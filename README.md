# libobs.NET
This is a C# wrapper for libobs. It's intention is to provide straightforward API for building around the libobs library, creating applications on the .NET platform.

This library is currently built around .NET 5 and LibObs 27.5.32.

## Development Notes
1. Currently supports only a very limited amount of features mainly for the purpose of recording/encoding. It is mainly built for the use of my personal project, [RePlays](https://github.com/lulzsun/RePlays).

2. Do not use this unless you understand the consequences of the API being directly exposed, (at the time of writing, there are no safety wrapper methods). You should only use this if you can handle the issues related to this.

3. For docs, you can reference from [obsproject's documentation](https://obsproject.com/docs/index.html). Naming conventions of methods/classes/etc. are 1:1 with the docs (because of note #2), for ease of use and straightforward library development.

Missing features that you would like to see? Submit an issue ticket!

## Install
Build libobs yourself or use this [prebuilt version 27.5.32](https://obsstudios3.streamlabs.com/libobs-windows64-release-27.5.32.7z) provided by Streamlabs.

If you are using the prebuilt version, this is what the file structure should (roughly) look like after you unzip:
```
- packed_build
    - bin
        - 64bit
            - obs.dll & ~dependencies/.dlls, etc. files~
    - cmake
    - data
    - include
    - obs-plugins
```

Using the `obs_net.example` project as an example, this is how the libobs files should be located under `Debug` folder in order for everything to work correctly when debugging.

```
- Debug
    - net5.0
        - data
        - obs-plugins
        - obs.dll & ~dependencies/.dlls, etc. files~
        - obs_net.example.exe
```

## TODO
- abstraction 
- type safety
- gc / memory management

## Special thanks to
[GoaLitiuM/libobs-sharp](https://github.com/GoaLitiuM/libobs-sharp) used some snippets and as reference

[FFFFFFFXXXXXXX/libobs-recorder](https://github.com/FFFFFFFXXXXXXX/libobs-recorder) used as learning reference

[stream-labs/obs-studio-node](https://github.com/stream-labs/obs-studio-node) used as learning reference
