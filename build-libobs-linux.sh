rm -rf obs-studio
git clone --recursive --depth 1 --branch 30.0.0 https://github.com/obsproject/obs-studio.git

cd obs-studio
rm -rf build
mkdir -p build && cd build
cmake -DCMAKE_BUILD_TYPE=Release -DLINUX_PORTABLE=ON -DCMAKE_INSTALL_PREFIX="../../build" \
    -DENABLE_BROWSER=OFF \
    -DENABLE_VLC=OFF \
    -DENABLE_UI=OFF \
    -DENABLE_SCRIPTING=OFF \
    -DENABLE_NEW_MPEGTS_OUTPUT=OFF \
    -DENABLE_RTMPS=OFF \
    -DENABLE_WEBRTC=OFF \
    -DENABLE_VST=OFF \
    -DENABLE_AJA=OFF \
    ..
make -j4 && make install
