cmake_minimum_required(VERSION 3.16)
project(CreateWindowExDemo)

# Set C++ standard
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# Set output directory
set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/bin)

# Create executable
add_executable(CreateWindowExDemo WIN32 main.cpp)

# Link Windows libraries
target_link_libraries(CreateWindowExDemo 
    user32 
    gdi32 
    comctl32
)

# Set compiler flags for Windows
if(MSVC)
    target_compile_options(CreateWindowExDemo PRIVATE /W4)
else()
    target_compile_options(CreateWindowExDemo PRIVATE -Wall -Wextra)
endif()

# Set Windows subsystem
if(MSVC)
    set_target_properties(CreateWindowExDemo PROPERTIES
        LINK_FLAGS "/SUBSYSTEM:WINDOWS"
    )
endif() 