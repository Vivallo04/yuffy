#!/bin/bash
set -euo pipefail

# Usage: ./create-macos-bundle.sh <version>
# Creates Yuffy.app universal macOS bundle from pre-built osx-x64 and osx-arm64 publish outputs.
# Must be run from the repository root.

VERSION="${1:-0.0.0}"
PROJECT_DIR="Yuffy"
APP_NAME="Yuffy"
PUBLISH_BASE="$PROJECT_DIR/bin/Release/net9.0"
BUNDLE_DIR="$APP_NAME.app"

echo "==> Creating macOS bundle for $APP_NAME v$VERSION"

# --- Clean previous bundle ---
rm -rf "$BUNDLE_DIR"

# --- Create .app directory structure ---
mkdir -p "$BUNDLE_DIR/Contents/MacOS"
mkdir -p "$BUNDLE_DIR/Contents/Resources"

# --- Create universal binary via lipo ---
echo "==> Creating universal binary (x64 + arm64)"
lipo -create \
    "$PUBLISH_BASE/osx-x64/publish/$APP_NAME" \
    "$PUBLISH_BASE/osx-arm64/publish/$APP_NAME" \
    -output "$BUNDLE_DIR/Contents/MacOS/$APP_NAME"
chmod +x "$BUNDLE_DIR/Contents/MacOS/$APP_NAME"

# --- Copy native libraries from both architectures ---
# MonoGame DesktopGL ships native .dylib files that need to be included
echo "==> Copying native libraries"
for rid in osx-x64 osx-arm64; do
    find "$PUBLISH_BASE/$rid/publish/" -name "*.dylib" -exec cp -n {} "$BUNDLE_DIR/Contents/MacOS/" \; 2>/dev/null || true
done

# --- Copy managed assemblies (DLLs) from one publish output ---
echo "==> Copying managed assemblies"
cp "$PUBLISH_BASE/osx-arm64/publish/"*.dll "$BUNDLE_DIR/Contents/MacOS/" 2>/dev/null || true
cp "$PUBLISH_BASE/osx-arm64/publish/"*.json "$BUNDLE_DIR/Contents/MacOS/" 2>/dev/null || true

# --- Copy Content ---
echo "==> Copying game content"
if [ -d "$PUBLISH_BASE/osx-arm64/publish/Content" ]; then
    cp -R "$PUBLISH_BASE/osx-arm64/publish/Content" "$BUNDLE_DIR/Contents/Resources/Content"
elif [ -d "$PROJECT_DIR/Content/bin/DesktopGL" ]; then
    cp -R "$PROJECT_DIR/Content/bin/DesktopGL" "$BUNDLE_DIR/Contents/Resources/Content"
else
    echo "WARNING: No Content directory found!"
fi

# --- Write Info.plist with version ---
echo "==> Writing Info.plist (version: $VERSION)"
sed "s/VERSION_PLACEHOLDER/$VERSION/g" build/Info.plist > "$BUNDLE_DIR/Contents/Info.plist"

# --- Generate .icns icon ---
echo "==> Generating app icon"
ICON_SOURCE=""
if [ -f "build/Icon.png" ]; then
    ICON_SOURCE="build/Icon.png"
elif [ -f "$PROJECT_DIR/Icon.ico" ]; then
    # Extract PNG from .ico using sips
    ICON_SOURCE="/tmp/yuffy_icon.png"
    sips -s format png "$PROJECT_DIR/Icon.ico" --out "$ICON_SOURCE" >/dev/null 2>&1 || true
fi

if [ -n "$ICON_SOURCE" ] && [ -f "$ICON_SOURCE" ]; then
    ICONSET_DIR="/tmp/$APP_NAME.iconset"
    rm -rf "$ICONSET_DIR"
    mkdir -p "$ICONSET_DIR"

    sips -z 16 16     "$ICON_SOURCE" --out "$ICONSET_DIR/icon_16x16.png"      >/dev/null 2>&1
    sips -z 32 32     "$ICON_SOURCE" --out "$ICONSET_DIR/icon_16x16@2x.png"   >/dev/null 2>&1
    sips -z 32 32     "$ICON_SOURCE" --out "$ICONSET_DIR/icon_32x32.png"      >/dev/null 2>&1
    sips -z 64 64     "$ICON_SOURCE" --out "$ICONSET_DIR/icon_32x32@2x.png"   >/dev/null 2>&1
    sips -z 128 128   "$ICON_SOURCE" --out "$ICONSET_DIR/icon_128x128.png"    >/dev/null 2>&1
    sips -z 256 256   "$ICON_SOURCE" --out "$ICONSET_DIR/icon_128x128@2x.png" >/dev/null 2>&1
    sips -z 256 256   "$ICON_SOURCE" --out "$ICONSET_DIR/icon_256x256.png"    >/dev/null 2>&1
    sips -z 512 512   "$ICON_SOURCE" --out "$ICONSET_DIR/icon_256x256@2x.png" >/dev/null 2>&1
    sips -z 512 512   "$ICON_SOURCE" --out "$ICONSET_DIR/icon_512x512.png"    >/dev/null 2>&1
    sips -z 1024 1024 "$ICON_SOURCE" --out "$ICONSET_DIR/icon_512x512@2x.png" >/dev/null 2>&1

    iconutil -c icns "$ICONSET_DIR" --output "$BUNDLE_DIR/Contents/Resources/$APP_NAME.icns"
    rm -rf "$ICONSET_DIR"
    echo "   Icon generated successfully"
else
    echo "   WARNING: No icon source found, skipping .icns generation"
fi

# --- Ad-hoc code sign ---
echo "==> Code signing (ad-hoc)"
find "$BUNDLE_DIR/Contents/MacOS" -name "*.dylib" -exec codesign --force -s - {} \;
codesign --force --deep -s - "$BUNDLE_DIR"

# --- Create archive ---
echo "==> Creating archive"
tar -czf "$APP_NAME-macos-universal.tar.gz" "$BUNDLE_DIR"

echo "==> Done! Created $APP_NAME-macos-universal.tar.gz"
ls -lh "$APP_NAME-macos-universal.tar.gz"
