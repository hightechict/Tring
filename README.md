# Tring

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
google.nl:http
google.nl:80
google.nl 80
google.nl htpp
8.8.8.8:80
8.8.8.8:http
8.8.8.8 80
8.8.8.8 http
```

Output example:

```bash
| Time              | IP              | Port  | Result  | Ping | Local Interface |
| 14:13:42-14:21:21 | 172.217.19.195  | 80    | OK      | -    | 10.100.100.155  |
```

## License

This work is licensed under the LGPL license, refer to the [COPYING.md][license] and [COPYING.LESSER.md][licenseExtension] files for details.

## Repository

The source code of this projecte can be found on [github](https://github.com/hightechict/Tring).

[license]: https://raw.githubusercontent.com/hightechict/Tring/develop/COPYING
[licenseExtension]: https://raw.githubusercontent.com/hightechict/Tring/develop/COPYING.LESSER
[globalTool]: https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools
