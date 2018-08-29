using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace api
{
	public static class TodoItems
	{
		private static AuthorizedUser GetCurrentUserName(TraceWriter log)
		{
			// On localhost claims will be empty
			string name = "Dev User";
			string upn = "dev@localhost";

			foreach (Claim claim in ClaimsPrincipal.Current.Claims)
			{
				if (claim.Type == "name")
				{
					name = claim.Value;
				}
				if (claim.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn")
				{
					upn = claim.Value;
				}
				//Uncomment to print all claims to log output for debugging
				//log.Verbose("Claim: " + claim.Type + " Value: " + claim.Value);
			}
			return new AuthorizedUser() {DisplayName = name, UniqueName = upn };
		}
		
		// Add new item
		[FunctionName("TodoItemAdd")]
		public static HttpResponseMessage AddItem(
			[HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todoitem")]HttpRequestMessage req,
			[DocumentDB("ServerlessTodo", "TodoItems")] out TodoItem newTodoItem,
			TraceWriter log)
		{
			// Get request body
			TodoItem newItem = req.Content.ReadAsAsync<TodoItem>().Result;
			log.Info("Upserting item: " + newItem.ItemName);
			if (string.IsNullOrEmpty(newItem.id))
			{
				// New Item so add ID and date
				log.Info("Item is new.");
				newItem.id = Guid.NewGuid().ToString();
				newItem.ItemCreateDate = DateTime.Now;
				newItem.ItemOwner = GetCurrentUserName(log).UniqueName;
			}
			newTodoItem = newItem;

			return req.CreateResponse(HttpStatusCode.OK, newItem);
		}

		// Get all items
		[FunctionName("TodoItemGetAll")]
		public static HttpResponseMessage GetAll(
		   [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todoitem")]HttpRequestMessage req,
		   [DocumentDB("ServerlessTodo", "TodoItems")] DocumentClient client, 
		   TraceWriter log)
		{
			var currentUser = GetCurrentUserName(log);
			log.Info("Getting all Todo items for user: " + currentUser.UniqueName);

			Uri collectionUri = UriFactory.CreateDocumentCollectionUri("ServerlessTodo", "TodoItems");

			var itemQuery = client.CreateDocumentQuery<TodoItem>(collectionUri).Where(i => i.ItemOwner == currentUser.UniqueName);

			var ret = new { UserName = currentUser.DisplayName, Items = itemQuery.ToArray() };

			return req.CreateResponse(HttpStatusCode.OK, ret);
		}

		// Delete item by id
		[FunctionName("TodoItemDelete")]
		public static async Task<HttpResponseMessage> DeleteItem(
		   [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "todoitem/{id}")]HttpRequestMessage req,
		   [DocumentDB("ServerlessTodo", "TodoItems")] DocumentClient client, string id,
		   TraceWriter log)
		{
			var currentUser = GetCurrentUserName(log);
			log.Info("Deleting document with ID " + id + " for user " + currentUser.UniqueName);

			Uri documentUri = UriFactory.CreateDocumentUri("ServerlessTodo", "TodoItems", id);

			try
			{
				var item = await client.ReadDocumentAsync<TodoItem>(documentUri);
				
				// Verify the user owns the document and can delete it
				if (item.Document.ItemOwner == currentUser.UniqueName)
				{
					await client.DeleteDocumentAsync(documentUri);
				}
				else
				{
					log.Warning("Document with ID: " + id + " does not belong to user " + currentUser.UniqueName);
				}
			}
			catch (DocumentClientException ex)
			{
				if (ex.StatusCode == HttpStatusCode.NotFound)
				{
					// Document does not exist or was already deleted
					log.Warning("Document with ID: " + id + " not found.");
				}
				else
				{
					// Something else happened
					throw ex;
				}
			}

			return req.CreateResponse(HttpStatusCode.NoContent);
		}
	}
}
