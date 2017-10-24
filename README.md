Asynchronous Command/Query library
===================================

This library contains interfaces used to be able to use messaging (commands, queries and events) in applications without tightly coupling it to a specific implementation.

## Example

Define a message:

```csharp
public class ActivateAccount
{
	public int AccountId {get; set; }
	public string ActivationKey { get; set; }
}
```

Invoke it:

```csharp
var msg = new ActivateAccount { AccountId = 35, ActivationKey = "dfkldsie93kcn22" };
await _messageBus.SendAsync(msg);
```

Handle it:

```csharp
public class ActivateAccountHandler : IMessageHandler<ActivateAccount>
{
    private readonly IAccountRepository _repository;
	
	public ActivateAccountHandler(IAccountRepository repository)
	{
		if (repository == null) throw new ArgumentNullException(repository);
		
		_repository = repository;
	}
	
	public async Task HandleAsync(IMessageContext context, ActivateAccount message)
	{
		var user = await _repository.Get(message.AccountId);
		user.Activate(message.ActivationKey);
		await _repository.UpdateAsync(user);
		
		await context.SendAsync(new AccountActivated(message.AccountId));
	}
}
```

Message handlers are fully isolated from the rest of the specification and therefore easy to test and maintain.


# Implementations

The following implementations exist.

## Bus 

The `MessageBus` and `QueryBus` currently have the following implementations:

* DependenyInjection: Use your favorite container to execute and queue messages.
 * Microsoft.Extensions.DependencyInjection: Currently under implementation
 * Griffin.Container: Currently under implementation

## Queues

* ADO.NET: Uses a table in your database to enqueue and dequeue messages (to get persistance).
* Azure ServiceBus: Under development

