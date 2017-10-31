The IRIS CCM - Codec Call Monitor
=================================

CCM is a platform for administration and management of an ACIP-compliant SIP
infrastructure for live radio broadcast.

Release plan
------------
The first release is mostly a matter of "getting the code licensed and out of
the door". We have gone through a lot of work to get the legal process done
with this release, and to get the code cleaned up from internal IP addresses,
domains and passwords. 

This process will continue and we plan a release 2.0 with more documentation,
less Swedish and maybe even some helpful installation scripts. At release 2.0
we will start working with Github.com as our main coding platform and you
will see all changes as they happen.

If you like to contribute documentation, local translations or code you are
more than welcome with a pull request on github.com.

Architecture
------------
CCM consists of a web application, a web service and a windows service built
with Microsoft .NET framework in C# storing data in a MySQL or MariaDB database.
Authentication for SIP is done using a Radius service. The CCM interacts
and manages accounts in the Radius service.

In production use, the CCM is by default running on two separate servers in 
different data centers. Data is replicated using MySQL replication. Kamailio
updates both CCM servers with current registration states as well as call states.

Load balancing between the two CCM servers can be done with external HTTP load
balancers, using DNS or virtual IP failover.

Platforms and tools
-------------------
CCM is build on a Microsoft Windows platform interacting with a Kamailio SIP server
running on a Linux server. 
CCM is developed in C# using Microsoft .NET Framework 4.5. ASP.NET MVC 5 is used for
the web services. Development is done in Microsoft Visual Studio 2013.

For databases, the Entity Framework v6 is used.

The CCM web
-----------

The web interface provides an overview of available (and registered) devices as well
as current broadcast sessions. A user can view registration IP addresses, device types
and for some devies, manage the device in the CCM. When searching, filters based on
region, type of device or other criteria may be applied.

When logging in as an administrator, account management is available - both CCM web logins
as well as SIP accounts. 

The IRIS discovery service
--------------------------
The IRIS discovery services, also known by the name "Active Phonebook", is used by the
connected devices to find other available devices to set up a live session. The service
has three main functions:
* Profiles: List available call profiles
* Filters: List available pre-defined search filters
* Devices: List available devices based on applied filters and profiles

All functions require a valid authentication.

Code modules
------------

* CCM.Core:	The core of the IRIS CCM platform. 
* CCM.Data:	Module for data storage. 
* CCM.Web: 	The web user interface (ASP.NET MVC 5 project)
* CCM.DiscoveryApi:	The IRIS discovery service (ASP.NET MVC 5 project)
* CCM.CodecControl:	A module to remotely manage devices by using vendor specific API services
* CCM.Tests:	Unit and integration tests
		Note: The unit tests are in working shape, but the rest of the tests may or
		may not work at this stage. Help is welcome!
* Radius.NET:	A modified version of Radius.NET from https://github.com/frontporch/Radius.NET

Installation of a test platform
===============================

CCM
---
1. Create MySQL database
2. Install windows tools needed
3. Create default root user
4. (to be added soon)

Radius
------
The CCM use a FreeRadius server with a MySQL backend.

Kamailio
--------
Please see the separate IRIS project for Kamailio


Feedback, bugs, comments?
=========================
* Please open an issue on Github
