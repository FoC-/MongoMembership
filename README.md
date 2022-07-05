MongoMembership
===============

[![build](https://github.com/FoC-/MongoMembership/actions/workflows/build.yml/badge.svg)](https://github.com/FoC-/MongoMembership/actions/workflows/build.yml) [![NuGet Version](https://img.shields.io/nuget/v/MongoMembership.svg)](https://nuget.org/packages/MongoMembership)

Contain ASP.NET providers for working with Membership, Role, Profile via MongoDB.

* [Nuget](#nuget)
* [Configuration](#configuration)
* [License](#license)

### Nuget
https://nuget.org/packages/MongoMembership

### Configuration
MongoMembership should been added and configured in your Web.config file
```xml          
    <configuration>
	<connectionStrings>
	    <add name="MONGOLAB_URI" connectionString="mongodb://localhost/MongoLab"/>
	    <add name="MongoUri" connectionString="mongodb://localhost/TestMongoMembershipProvider"/>
	</connectionStrings>
        <system.web>
            <membership defaultProvider="MongoMembershipProvider">
                <providers>
                     <clear/>
                     <add name="MongoMembershipProvider"
                          type="MongoMembership.Providers.MongoMembershipProvider"
                          connectionStringKeys="MongoUri"
                          enablePasswordRetrieval="false"
                          enablePasswordReset="true"
                          requiresQuestionAndAnswer="false"
                          requiresUniqueEmail="false"
                          maxInvalidPasswordAttempts="5"
                          minRequiredPasswordLength="6"
                          minRequiredNonalphanumericCharacters="0"
                          passwordAttemptWindow="10"
                          applicationName="/" />
                </providers>
            </membership>

            <profile defaultProvider="MongoProfileProvider">
                <providers>
                    <clear/>
                    <add name="MongoProfileProvider"
                         type="MongoMembership.Providers.MongoProfileProvider"
                         connectionStringKeys="MongoUri"
                         applicationName="/" />
                </providers>
            </profile>

            <roleManager defaultProvider="MongoRoleProvider">
                <providers>
                    <clear/>
                    <add name="MongoRoleProvider"
                         type="MongoMembership.Providers.MongoRoleProvider"
                         connectionStringKeys="MongoUri"
                         applicationName="/" />
                </providers>
            </roleManager>
        </system.web>
    </configuration>
```

Please notice
-------------
Each provider have new attribute _connectionStringKeys_ which could contain coma separated keys(links) to the keys in _appSettings_ section. Order of keys is important. Each key in _appSettings_ should be connection string to MongoDB or should absent in config. If no connection string is configured then default: "mongodb://localhost/MongoMembership" will be used.

### License
MongoMembership is free software distributed under the terms of MIT License (see LICENSE.txt) these terms do not apply to other 3rd party tools, utilities or code which may be used to develop this application.

# P.S.
Do not forget to give your feedback about this project.
