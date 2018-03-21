Installation
------------------------------


### Via nuget

Just find the package postmen, and add it to your project.

### Via source code.

```
git clone file
```

Then in your solution click in add a existing project, and select the clone one. Don't forget to add the reference in your project.

Usage
----------------------------

### Quick Start

The following code list all the labels by API key.

```
using System;
using Postmen_sdk_NET;
using Newtonsoft.Json.Linq

namespace Postmen_sdk_NET
{
    class Test
    {
        static void Main(string[] args)
        {
            // Just read the key from a file
            string key_sandbox = System.IO.File.ReadAllText(@"\\psf\Home\Documents\postmen_sandbox_key.txt");

            HandlerAPI handler = new HandlerAPI(api_key: key_sandbox, endpoint: "https://sandbox-api.postmen.com");
            // Get the resource 'labels'
            JObject result = handler.get(resource: "labels");

            Console.WriteLine(result);
        }
    }
}
```

### Get API object

Import postmen module and obtain API handler object. Pass a valid API key and region, or API key and endpoint.

```
HandlerAPI handler = new HandlerAPI(api_key: 'YOUR_API_KEY', endpoint: "https://sandbox-api.postmen.com");

// Or using region (sandbox, production)

HandlerAPI handler = new HandlerAPI(api_key: 'YOUR_API_KEY', region: "sandbox");
```

### Make API calls

**Common method to make API call (normally you shouldn't use it directly):**

```
public JObject call(string method, string path, JObject body = null, string query = "", bool retry = true) {..}
```

**HTTP-methods acces to direclty map API docs to SDK calls:**

```
public JObject GET(string path, string query = "", bool retry = true) {..}

public JObject POST(string path, JObject body = null, string query = "", bool retry = true) {..}

public JObject PUT(string path, JObject body = null, string query = "", bool retry = true) {..}

public JObject DELETE(string path, string query = "", bool retry = truej) {..}
```

**User-friendly API access methods:**

get:
- List all resources -> only inform resource as for example 'labels'. [docs] (https://docs.postmen.com/#label-list-all-labels)
- get a resource -> inform 
- resource (e.g. 'lables') and the id of the resource you want to get. [docs] (https://docs.postmen.com/#label-retrieve-a-label)
 
```
 public JObject get(string resource, string id = "", string query ="", bool retry = true, bool safe = false) {..}
```
 
create:
- Create a new resource -> inform resource (e.g. 'labels'), and the body as a JObject in the payload. [docs] (https://docs.postmen.com/#label-create-a-label)
 
 ```
 public JObject create(string resource, JObject payload, string query = "", bool retry = true, bool safe = false {..}
 ```

cancel:
- Delete/Cancel a resource -> inform resource (e.g. 'labels'), and the id of the element you want to cancel. [docs](https://docs.postmen.com/#label-cancel-a-label) 

```
public JObject cancel(string resource, string id = "", string query ="", bool retry = true, bool safe = false) {..}
```

Apart from the common fields we always have:
- query: By default is empty, is the query we send to the API, e.g. 'created_at_min=2015-01-01T00:00:00+00:00'.
- retry: By default true, in case of error we will retry 4 times, to make sure there was not a network problem.

### Dealing with JObject

As you can see, when you need to create a new element you will need to create a JObject, there are different ways of doing it, for example:

```
// Using dynamic JObjects (you will need to add the reference to Microsoft.CSharp)
using Newtonsoft.Json.Linq;

dynamic element2 = new JObject();
element2.something = "second element"

//Using pure JObjects
JObject element1 = new JObject();
element1.Add("something", "first element");

//using string
string json = @"{something: 'first element's}"
JObject example1 =  JObject.Parse(json);
```

### Examples

```
using System;
using Postmen_sdk_NET;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;


namespace Postmen_sdk_NET
{
	class Test
	{
		static void Main(string[] args)
		{
			string production_key = System.IO.File.ReadAllText(@"\\psf\Home\Documents\postmen_production_key.txt");
			string account_dhl_production = System.IO.File.ReadAllText(@"\\psf\Home\Documents\postmen_production_account_key_dhl.txt"); 
			HandlerAPI handler_production_sandbox = new HandlerAPI(api_key: production_key, region: "production");

			JArray shipper_account =  new JArray();
			JObject shipper_account_element = new JObject();
			shipper_account_element.Add("id",account_dhl_production);
			shipper_account.Add(shipper_account_element);

			JObject address1 = JObject.Parse(@"{
				city: 'Los Angeles',
				state: 'CA',
				postal_code: '90001',
				country: 'USA'
			}");
			JObject address2 = JObject.Parse(@"{
				city:'Beverly Hills',
				postal_code:'90209',
				state:'CA',
				country:'USA'
			}");
			JArray parcels1 = JArray.Parse(@"[
				{
					description: 'iMac (Retina 5K, 27-inch, Late 2014)',
					box_type: 'custom',
					weight: {value: 9.54, unit: 'kg'},
					dimension: {width: 65, height: 52, depth: 21, unit: 'cm'},
					items:[
						{
							description: 'iMac (Retina 5K, 27-inch, Late 2014)',
							origin_country: 'USA',
							quantity: 1,
							price: {amount:1999,currency:'USD'},
							weight: {value:9.54,unit:'kg'},
							sku: 'imac2014'
						}
					]
				}
			]");

			// Create rates
			dynamic request_data = new JObject();
			request_data.shipper_accounts = shipper_account;
			request_data.async = false;
			request_data.shipment = new JObject();
			request_data.shipment.ship_from = address1;
			request_data.shipment.ship_to = address2;
			request_data.shipment.parcels = parcels1;

			JObject result = handler_production_sandbox.create(resource: "rates", payload: request_data);

			// Access the status:
			Console.WriteLine(result["data"]["status"]);
			return;
			string key_sandbox = System.IO.File.ReadAllText(@"\\psf\Home\Documents\postmen_sandbox_key.txt");

			Console.WriteLine(key_sandbox);
			HandlerAPI handler = new HandlerAPI(api_key: key_sandbox, endpoint: "https://testing-sandbox-api.postmen.io");
			
			result = handler.get(resource: "labels");

			Console.WriteLine(result);

		}

	}

}
```


## Release History
####2015-09-16 v0.0.2
* Solving issues witht the Nuget package.
 
####2015-09-15 v0.0.1
* Beta version

## License
Copyright (c) 2015 Aftership  
Licensed under the MIT license.
