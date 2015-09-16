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
			string production_ap_southeast_key = System.IO.File.ReadAllText(@"\\psf\Home\Documents\postmen_production_ap_southeast_key.txt");
			string account_dhl_production = System.IO.File.ReadAllText(@"\\psf\Home\Documents\postmen_production_ap_southeast_account_key_dhl.txt"); 
			HandlerAPI handler_production_sandbox = new HandlerAPI(api_key: production_ap_southeast_key, region: "ap-southeast");

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

			return;

			string json = @"{
				async:false,
				shipper_accounts: [
					{
						id: '000000000000-b754-4981-bee8-e233f79fd53a'
					}
				],
				is_document: false,
				shipment:{
					ship_from: {
						contact_name: 'Jameson McLaughlin',
						company_name: 'Bode, Lind and Powlowski',
						street1: '8918 Borer Ramp',
						city: 'Los Angeles',
						state: 'CA',
						postal_code: '90001',
						country: 'USA',
						type: 'business'
				   },
				   ship_to:{
					   contact_name: 'Dr. Moises Corwin',
					   phone: '1-140-225-6410',
						email:'Giovanna42@yahoo.com',
						street1:'28292 Daugherty Orchard',
						city:'Beverly Hills',
						postal_code:'90209',
						state:'CA',
						country:'USA',
						type:'residential'
					},
					parcels: [
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
					]
				}
			}"; 

			JObject example1 =  JObject.Parse(json);

			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			// dynamic need a reference to Microsfot.CSharp


			dynamic address_from = new JObject();

			address_from.contact_name = "Jameson McLaughlin";
			address_from.company_name = "Bode; Lind and Powlowski";
			address_from.street1 = "8918 Borer Ramp";
			address_from.city = "Los Angeles";
			address_from.state = "CA";
			address_from.postal_code = "90001";
			address_from.country = "USA";
			address_from.type = "business";

			dynamic example2 = new JObject();

			example2.ship_from = address_from;

			ArrayList array_example = new ArrayList();

			array_example.Add("algo");
			array_example.Add("something else");

			JObject element1 = new JObject();

			element1.Add("something", "first element");

			dynamic element2 = new JObject();

			element2.something = "second element";

			JObject[] array_jobject = { element1, element2 };
			JArray array_json = new JArray();
			array_json.Add(element1);
			array_json.Add(element2);



			example2.example_array = array_json; 

			Console.WriteLine(example2.ToString());
			Console.WriteLine("------------------------------------");


			string error_json = @"{ 
				meta: {
					code: 1234,
					message: 'this is an error',
					retryable: false,
					details: [{
						path: 'data.shipment.parcels.0.items.0.price',
						info: 'The required property \'currency\' is missing'
					}]
				},
				data: 'something'
		   }";	

			PostmenException error = PostmenException.FactoryMethod(JObject.Parse(error_json));

			Console.WriteLine(error);

		}
	}
}

