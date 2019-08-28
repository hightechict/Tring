# Tring

[![Build Status](https://dev.azure.com/basbossink0470/Tring/_apis/build/status/hightechict.Tring?branchName=develop)](https://dev.azure.com/basbossink0470/Tring/_build/latest?definitionId=2&branchName=develop)
[![nuget](https://img.shields.io/nuget/v/Tring)](https://www.nuget.org/packages/tring/)
![licenceTag](https://img.shields.io/github/license/hightechict/Tring.svg)

Tring can be used to quickly test a TCP connection.
It consists of an executable that can be installed as a [`dotnet` global tool][globalTool].

## Installation

Tring can be installed as a [`dotnet` global tool][globalTool] by using the following command:

```bash
$ dotnet tool install --global Tring
```

## Usage

To use Tring run the command `Tring` and supply a ip or url with a port, if the url contains a protocol the port will be inferred.
Adding -w or --watch will keep checking the request every second.

Accepted input expampels:
```bash
http://google.nl
http://google.nl:443
http://google.nl:443/example
google.nl:http
google.nl:80
8.8.8.8:80
8.8.8.8:http
```

Output example:

```bash
| Time              | IP              | Port  | Connect | Ping    | Local Interface | Protocol | Hostname
| 14:38:05-14:38:07 | 172.217.20.99   | 80    | 7 ms    | -       | 10.100.100.112  | http     | google.nl
```

## License

This work is licensed under the LGPL license, refer to the [COPYING.md][license] and [COPYING.LESSER.md][licenseExtension] files for details.

## Repository

The source code of this projecte can be found on [github](https://github.com/hightechict/Tring).

[license]: https://raw.githubusercontent.com/hightechict/Tring/develop/COPYING
[licenseExtension]: https://raw.githubusercontent.com/hightechict/Tring/develop/COPYING.LESSER
[globalTool]: https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools
