rm -rf obs-studio
git clone --recursive --depth 1 --branch 30.1.1 https://github.com/obsproject/obs-studio.git

cd obs-studio

wget -nc ! https://gitlab.archlinux.org/archlinux/packaging/packages/obs-studio/-/raw/30.1.2-2/0001-obs-ffmpeg-Fix-incompatible-pointer-types-with-FFmpe.patch
patch -Np1 < 0001-obs-ffmpeg-Fix-incompatible-pointer-types-with-FFmpe.patch

rm -rf build
mkdir -p build && cd build
cmake -B build \
    -DCALM_DEPRECATION=ON \
    -DCMAKE_INSTALL_PREFIX="../../build" \
    -DLINUX_PORTABLE=ON \
    -DENABLE_BROWSER=OFF \
    -DENABLE_VLC=OFF \
    -DENABLE_UI=OFF \
    -DENABLE_SCRIPTING=OFF \
    -DENABLE_NEW_MPEGTS_OUTPUT=OFF \
    -DENABLE_RTMPS=OFF \
    -DENABLE_WEBRTC=OFF \
    -DENABLE_VST=OFF \
    -DENABLE_AJA=OFF \
    -Wno-dev \
    ..
cmake --build build
cmake --install build
