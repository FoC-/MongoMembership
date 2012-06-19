MongoMembership
===============

Contain ASP.NET providers for working with Membership, Role, Profile via MongoDB.

## Configuration
MongoMembership should been added and configured in your Web.config file
          
    <configuration>
	<appSettings>
	    <add key="MONGOLAB_URI" value="mongodb://localhost/MongoLab"/>
	    <add key="MongoUri" value="mongodb://localhost/TestMongoMembershipProvider"/>
	</appSettings>
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

Please notice
-------------
Each provider have new attribute _connectionStringKeys_ which could contain coma separated keys(links) to the _appSettings_ section which contain all or one of that keys and value will be connection string to MongoDB

# P.S.
Do not forget to give your feedback about this project.