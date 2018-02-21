# cpls

This tool exports and imports JSON configuration files to and from configuration api.

## Installation

Download the latest release's zip file and expand it into a folder. This folder contains a binary called PLS.exe, it's your main entry point.

## Usage

In order to use it you must open a terminal client and cd into the folder in which PLS.exe can be found. Then you can use the ```--help``` or ```-h``` option to get information about the available commands.

## Commands

### config

This command concerns the configuration api. It has two sub-commands which are:

#### export/import

```PLS.exe config (export|import) [url] [login] [password] [outputfile]```
This exports an application into a json file or import a json file into an application.

The arguments are:
- [URL]: the url of the tenant you want to work with. This url is formatted as follow: ```http(s)://<host>/<adminName>/api/<tenant>```.
For example: https://productevo.corp.alcuin.com/MDC_ENSSUPAdmin_EVO/api/MDC_ENSSUP_EVO
- [LOGIN]: your admin's login
- [PASSWORD]: your admin's password
- [OUTPUTFILE]: the file in which you want to put the export (it can be relative to PLS.exe) or from which you want to import
