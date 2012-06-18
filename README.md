MongoAccounting
===============

## Contain ASP.NET providers for working with Membership, Role, Profile via MongoDB.

## Configuration
MongoAccounting should been added and configured in your Web.config file:

<configuration>
	<appSettings>
	    <add key="LOCALHOST_test" value="mongodb://localhost/TestMongoMembershipProvider"/>
		<add key="MONGOLAB_URI" value="mongodb://localhost/MongoLab"/>
	</appSettings>
  	<system.web>
		<membership defaultProvider="MongoMembershipProvider">
			<providers>
				<clear/>
				<add name="MongoMembershipProvider" applicationName="MyApp" type="MongoAccounting.Providers.MongoMembershipProvider, MongoAccounting, Version=1.0.0.0, Culture=neutral" connectionStringKeys="LOCALHOST_test,MONGOLAB_URI" />
			</providers>
		</membership>
		<profile>
			<providers>
				<clear />
			</providers>
		</profile>
		<roleManager enabled="true" defaultProvider="MongoRoleProvider">
			<providers>
				<clear />
				<add name="MongoRoleProvider" applicationName="MyApp" type="MongoAccounting.Providers.MongoRoleProvider, MongoAccounting, Version=1.0.0.0, Culture=neutral" connectionStringKeys="LOCALHOST_test,MONGOLAB_URI"/>
			</providers>
		</roleManager>
	<system.web>
</configuration>
