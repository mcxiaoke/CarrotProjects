# Carrot VS Number

Tool for Automatic change  AssemblyFileVersion before Visual Studio Build, for C# projects.

# 版本号修改工具

用于Visual Studio 构建，按照预定规则自动修改 AssemblyFileVersion 版本号

# Command Line Usage:

```

  -f, --file      AssemblyInfo.cs file to change version number

  -b, --backup    bakup old AssemblyInfo.cs file

  -y, --yes       use this option to do real version number change

  -i, --inc       set version number by integer autoincrement

  -d, --days      set version number by days from 2022-01-01

  -v, --value     (Default: -1) set version number by command argument (0~65535)

  --help          Display this help screen.

  --version       Display version information.
```