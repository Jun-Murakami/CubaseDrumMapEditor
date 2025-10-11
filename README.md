# CubaseDrumMapEditor

A simple drum map editor for Cubase. Available for MacOS and Windows.  
Cubase 用のシンプルなドラムマップエディターです。
MacOS と Windows で利用可能.  
<img width="954" alt="スクリーンショット 2023-08-07 23 56 51" src="https://github.com/Jun-Murakami/CubaseDrumMapEditor/assets/126404131/f98dbef8-856d-4171-8356-77c5bd0cc037">

### Features

- Supports .csv import and export for editing in other software (e.g. Excel).
- Multiple simultaneous software launches are possible.

### 特徴

- .csv のインポートとエクスポートに対応し、ほかのソフトウェアで編集可能(エクセルなど)。
- ソフトウェアの複数同時起動が可能。

For more information (Japanese)
https://note.com/junmurakami/n/n13650982fc7f

## macOS ビルド & 公開フロー

macOS 版は `build.sh` でビルド・署名・ノータライズまで自動化しています（zsh 前提）。事前に以下を準備してください。

- .NET SDK 6 以降 (`dotnet` コマンドが利用可能なこと)
- Xcode Command Line Tools（`xcrun`, `stapler` などが使える状態）
- Developer ID Application 証明書（Keychain にインポート済み）
- Apple Notary API キー環境変数
  ```bash
  export APPLE_API_KEY_PATH="/path/to/AuthKey.p8"
  export APPLE_API_KEY_ID="XXXXXXXXXX"
  export APPLE_API_ISSUER="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
  ```

### 実行方法

```bash
cd CubaseDrumMapEditor
./build.sh
```

デフォルトでは以下を順番に実行します（.NET 9 SDK 前提）。

1. `dotnet publish` による macOS 用バイナリの生成（`RUNTIME=osx-x64`）
2. `.app` バンドルの組み立て（`Info.plist` と `avalonia-logo.icns` を配置）
3. Universal Binary 化（既定で `osx-arm64` もビルドし、実行ファイルと `.dylib` を `lipo` で結合）
4. `codesign` によるバンドル内ファイルとアプリ本体の署名
5. `hdiutil` で DMG 作成（既定で `bin/Release/CubaseDrumMapEditor.dmg` に配置し、DMG 内に `/Applications` ショートカットを同梱）
6. `notarytool` で DMG をノータライズ送信し、`stapler` でチケットを適用
7. 中間生成物（`.app` など）を削除し、DMG のみを残す

### オプション

環境変数で挙動をカスタマイズできます。

- `FRAMEWORK` : 例 `net8.0`。既定は `net9.0` で、`bin/Release/<framework>/<runtime>/publish` を参照
- `RUNTIME` : 例 `osx-arm64`。ユニバーサルビルド時は一次ビルド対象として使用
- `UNIVERSAL` : 既定 `1`。`0` にすると指定ランタイムのみをビルドし、`lipo` や追加アーキテクチャの発行をスキップ
- `OUTPUT_DIR` : 既定 `${projectRoot}/bin/Release`。生成した DMG を配置するフォルダ
- `DMG_NAME` : 既定 `CubaseDrumMapEditor.dmg`
- `DMG_VOLUME_NAME` : `hdiutil` でマウントした際に表示されるボリューム名の既定値
- `SKIP_BUILD=1` : 既存の `bin/.../publish` を流用して署名〜ノータライズのみ実行
- `SKIP_SIGN=1` / `SKIP_NOTARIZE=1` : 該当工程をスキップ
- `NOTARY_PROFILE` : 事前に `xcrun notarytool store-credentials` で保存したプロファイル名
- `CODESIGN_IDENTITY` : 既定以外の署名 ID を利用したい場合に指定

旧 `sign.sh` は互換性維持のため `build.sh` を呼び出すラッパーになっています。
