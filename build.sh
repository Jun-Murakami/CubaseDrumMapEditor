#!/usr/bin/env zsh

set -euo pipefail

SCRIPT_DIR="${0:a:h}"
cd "$SCRIPT_DIR"

APP_NAME=${APP_NAME:-CubaseDrumMapEditor.app}
DMG_NAME=${DMG_NAME:-CubaseDrumMapEditor.dmg}
DMG_VOLUME_NAME=${DMG_VOLUME_NAME:-"Cubase DrumMap Editor"}
OUTPUT_DIR=${OUTPUT_DIR:-"${SCRIPT_DIR}/bin/Release"}
CONFIGURATION=${CONFIGURATION:-Release}
FRAMEWORK=${FRAMEWORK:-net9.0}
RUNTIME=${RUNTIME:-osx-x64}
ENTITLEMENTS=${ENTITLEMENTS:-MyAppEntitlements.entitlements}
CODESIGN_IDENTITY=${CODESIGN_IDENTITY:-"Developer ID Application: Jun Takahashi (AQDS3A93CQ)"}
DOTNET_FLAGS=(${DOTNET_FLAGS:-})

UNIVERSAL=${UNIVERSAL:-1}
SKIP_BUILD=${SKIP_BUILD:-0}
SKIP_SIGN=${SKIP_SIGN:-0}
SKIP_NOTARIZE=${SKIP_NOTARIZE:-0}

PUBLISH_BASE="bin/${CONFIGURATION}/${FRAMEWORK}"
PUBLISH_DIR="${PUBLISH_BASE}/${RUNTIME}/publish"
DMG_PATH="${OUTPUT_DIR}/${DMG_NAME}"

MACOS_DIR="${APP_NAME}/Contents/MacOS"
RESOURCES_DIR="${APP_NAME}/Contents/Resources"

APPLE_API_KEY_PATH=${APPLE_API_KEY_PATH:-}
APPLE_API_KEY_ID=${APPLE_API_KEY_ID:-}
APPLE_API_ISSUER=${APPLE_API_ISSUER:-}
NOTARY_PROFILE=${NOTARY_PROFILE:-}

function info() {
  print -- "[INFO] $1"
}

function warn() {
  print -- "[WARN] $1" >&2
}

function require_cmd() {
  if ! command -v "$1" >/dev/null 2>&1; then
    print -- "[ERROR] Command '$1' not found. Please install it and retry." >&2
    exit 127
  fi
}

function build_runtime() {
  local runtime="$1"
  info "Building runtime ${runtime}"
  dotnet restore -r "$runtime" "${DOTNET_FLAGS[@]}"
  dotnet publish -c "$CONFIGURATION" -r "$runtime" \
    --self-contained true -p:PublishSingleFile=true "${DOTNET_FLAGS[@]}"
}

function assemble_app() {
  local runtime="$1"
  local publish_dir="${PUBLISH_BASE}/${runtime}/publish"
  if [[ ! -d "$publish_dir" ]]; then
    print -- "[ERROR] Publish directory not found: $publish_dir" >&2
    exit 1
  fi
  info "Assembling app bundle for ${runtime}"
  rm -rf "$APP_NAME"
  mkdir -p "$MACOS_DIR" "$RESOURCES_DIR"
  cp -R "${publish_dir}/." "$MACOS_DIR"
  cp Info.plist "${APP_NAME}/Contents/"
  cp Assets/avalonia-logo.icns "$RESOURCES_DIR/"
  find "$MACOS_DIR" -name '*.pdb' -delete
}

function create_universal_binary() {
  local primary_runtime="$1"
  local secondary_runtime
  case "$primary_runtime" in
    osx-x64) secondary_runtime="osx-arm64" ;;
    osx-arm64) secondary_runtime="osx-x64" ;;
    *)
      warn "Universal build requested but primary runtime ${primary_runtime} is unsupported for universal merging. Skipping."
      return
      ;;
  esac

  local secondary_publish="${PUBLISH_BASE}/${secondary_runtime}/publish"
  if (( SKIP_BUILD == 0 )); then
    info "Building secondary runtime ${secondary_runtime} for universal binary"
    build_runtime "$secondary_runtime"
  elif [[ ! -d "$secondary_publish" ]]; then
    print -- "[ERROR] Secondary publish directory not found: $secondary_publish" >&2
    exit 1
  fi

  if [[ ! -d "$secondary_publish" ]]; then
    print -- "[ERROR] Missing secondary runtime output: $secondary_publish" >&2
    exit 1
  fi

  info "Creating universal binaries"
  local primary_publish="${PUBLISH_BASE}/${primary_runtime}/publish"
  local mach_filter='Mach-O'
  if [[ ! -x /usr/bin/lipo ]]; then
    print -- "[ERROR] /usr/bin/lipo not available on this system." >&2
    exit 1
  fi
  if ! command -v file >/dev/null 2>&1; then
    print -- "[ERROR] 'file' command is required to create universal binaries." >&2
    exit 1
  fi

  find "$MACOS_DIR" -type f ! -path "*/.*" -print0 | while IFS= read -r -d '' target; do
    local rel_path="${target#$MACOS_DIR/}"
    local secondary_candidate="${secondary_publish}/${rel_path}"
    if [[ ! -f "$secondary_candidate" ]]; then
      continue
    fi

    local target_info secondary_info
    target_info=$(/usr/bin/lipo -info "$target" 2>/dev/null || printf '')
    secondary_info=$(/usr/bin/lipo -info "$secondary_candidate" 2>/dev/null || printf '')
    if [[ "$target_info" != *"architecture"* && "$target_info" != *"are"* ]]; then
      continue
    fi

    local target_has_x86=0 target_has_arm=0
    local secondary_has_x86=0 secondary_has_arm=0
    [[ "$target_info" == *"x86_64"* ]] && target_has_x86=1
    [[ "$target_info" == *"arm64"* ]] && target_has_arm=1
    [[ "$secondary_info" == *"x86_64"* ]] && secondary_has_x86=1
    [[ "$secondary_info" == *"arm64"* ]] && secondary_has_arm=1

    if (( target_has_x86 && target_has_arm )); then
      continue
    fi
    if (( secondary_has_x86 && secondary_has_arm )); then
      cp "$secondary_candidate" "$target"
      chmod +x "$target"
      continue
    fi

    if (( target_has_x86 == 0 && secondary_has_x86 == 0 )); then
      continue
    fi
    if (( target_has_arm == 0 && secondary_has_arm == 0 )); then
      continue
    fi

    local temp_file="${target}.universal"
    /usr/bin/lipo "$target" "$secondary_candidate" -create -output "$temp_file"
    mv "$temp_file" "$target"
    chmod +x "$target"
  done
}

function sign_payloads() {
  if (( SKIP_SIGN == 1 )); then
    warn "Skipping codesign as requested"
    return
  fi

  require_cmd codesign
  info "Signing nested executables"
  find "$MACOS_DIR" -type f ! -path "*/.*" \( -perm -111 -or -name "*.dylib" -or -name "*.so" \) -print0 |
    xargs -0 -I{} codesign --force --timestamp --options runtime \
      --entitlements "$ENTITLEMENTS" --sign "$CODESIGN_IDENTITY" "{}"

  info "Signing app bundle"
  codesign --force --timestamp --options runtime \
    --entitlements "$ENTITLEMENTS" --sign "$CODESIGN_IDENTITY" "$APP_NAME"

  info "Verifying codesign"
  codesign --verify --deep --strict --verbose=2 "$APP_NAME"
}

function create_dmg() {
  require_cmd hdiutil
  info "Creating DMG ${DMG_PATH}"
  local staging_dir="${SCRIPT_DIR}/.dmg-staging"
  rm -rf "$staging_dir"
  mkdir -p "$staging_dir"
  cp -R "$APP_NAME" "$staging_dir/"
  ln -sf /Applications "$staging_dir/Applications"
  mkdir -p "$OUTPUT_DIR"
  rm -f "$DMG_PATH"
  /usr/bin/hdiutil create -volname "$DMG_VOLUME_NAME" -srcfolder "$staging_dir" -ov -format UDZO "$DMG_PATH"
  rm -rf "$staging_dir"
}

function notarize_dmg() {
  if (( SKIP_NOTARIZE == 1 )); then
    warn "Skipping notarization as requested"
    return
  fi

  require_cmd xcrun

  local notary_flags=()
  if [[ -n "$NOTARY_PROFILE" ]]; then
    notary_flags+=(--keychain-profile "$NOTARY_PROFILE")
  else
    if [[ -z "$APPLE_API_KEY_PATH" || -z "$APPLE_API_KEY_ID" || -z "$APPLE_API_ISSUER" ]]; then
      warn "Notarization skipped: APPLE_API_* variables or NOTARY_PROFILE not set."
      return
    fi
    notary_flags+=(--key "$APPLE_API_KEY_PATH" --key-id "$APPLE_API_KEY_ID" --issuer "$APPLE_API_ISSUER")
  fi

  info "Submitting ${DMG_PATH} to Apple notary service"
  xcrun notarytool submit "$DMG_PATH" --wait "${notary_flags[@]}"

  info "Stapling ticket to DMG"
  xcrun stapler staple "$DMG_PATH"
  xcrun stapler validate "$DMG_PATH"

  info "Removing intermediate app bundle"
  rm -rf "$APP_NAME"
}

if (( SKIP_BUILD == 0 )); then
  require_cmd dotnet
  build_runtime "$RUNTIME"
else
  warn "Skipping dotnet build as requested"
fi

assemble_app "$RUNTIME"

if (( UNIVERSAL == 1 )); then
  create_universal_binary "$RUNTIME"
fi

sign_payloads
create_dmg
notarize_dmg

info "All done. DMG: ${DMG_PATH}"