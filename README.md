# Guffi2

SMS þjónusta Tertugallerí Myllunar

## Configuration
Guffi2.exe.config

```c#
<ApplicationSettings>
		<add key="ApplicationName" value="Guffi2"/>
        //SMS http string variables
		<add key="username" value="notendanafn"/>
		<add key="password" value="pw"/>
		<add key="from" value="Fyrirtæki ehf"/>
		<add key="klukkitimidags" value="12"/>
        //DataBase log in variables
		<add key="dataSource" value="dbs"/>
		<add key="databaseCatalog" value="ubc"/>
		<add key="databaseUsername" value="dbun"/>
		<add key="databasePassword" value="dbpw"/>

</ApplicationSettings>'
```

## Installation

Via Developer CMD for VS 2019
```bash
installutil Guffi2.exe
```



## License
hrafn@isam.is
