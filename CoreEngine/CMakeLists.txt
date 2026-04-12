cmake_minimum_required(VERSION 3.20)
project(TypesettingCore VERSION 1.0.0 LANGUAGES CXX)

# Set C++ standard
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# Build as shared library (DLL)
add_library(TypesettingCore SHARED
    src/InteropInterface.cpp
    src/TypesettingEngine.cpp
    src/HarfBuzzWrapper.cpp
)

# Include directories
target_include_directories(TypesettingCore PUBLIC
    ${CMAKE_CURRENT_SOURCE_DIR}/include
)

# Export all symbols for DLL
set_target_properties(TypesettingCore PROPERTIES
    WINDOWS_EXPORT_ALL_SYMBOLS ON
    RUNTIME_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/bin
    LIBRARY_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/lib
    ARCHIVE_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/lib
)

# Placeholders for HarfBuzz and ICU
# TODO: Uncomment and configure these when HarfBuzz and ICU are installed
# find_package(PkgConfig REQUIRED)
# pkg_check_modules(HARFBUZZ REQUIRED harfbuzz)
# target_include_directories(TypesettingCore PRIVATE ${HARFBUZZ_INCLUDE_DIRS})
# target_link_libraries(TypesettingCore PRIVATE ${HARFBUZZ_LIBRARIES})

# TODO: Add ICU when available
# find_package(ICU REQUIRED COMPONENTS uc i18n)
# target_link_libraries(TypesettingCore PRIVATE ICU::uc ICU::i18n)

# Platform-specific settings
if(WIN32)
    target_compile_definitions(TypesettingCore PRIVATE
        WIN32_LEAN_AND_MEAN
        NOMINMAX
        TYPSETTINGCORE_EXPORTS
    )
endif()

# Install targets
install(TARGETS TypesettingCore
    RUNTIME DESTINATION bin
    LIBRARY DESTINATION lib
    ARCHIVE DESTINATION lib
)

install(FILES include/TypesettingEngine.h DESTINATION include)