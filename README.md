Asynchronous Command/Query separation library definition.
=============

Contains all interfaces which are required to create a fully functional Command/Query separation library.

The purpose of the library is to allow you to start small and refactor your application with minimal changes
when the requirement to scale arises. 

i.e. it do only contain the interfaces that you need to start using Command/query. By using the interfaces you can at any time switch implementation (like from using an inversion control container to start sending commands over a message bus)

Existing imlementations (in other libraries)

* Via TCP
* Azure Service Bus
* Autofac
* Reflection based


	
