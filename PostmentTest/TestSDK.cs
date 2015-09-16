using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Postmen_sdk_NET;
using Newtonsoft.Json.Linq;

namespace PostmentTest
{
    [TestClass]
    public class TestSDK
    {

        HandlerAPI handler_production_sandbox;

        JObject address1;
        JObject address2;
        JObject address_wrong_address;
        JArray parcels1;
        JArray shipper_account_production_sandbox_array;
        JObject shipper_account_production_sandbox;
        const string endpoint_production_sandbox = "https://sandbox-api.postmen.com";

        static bool firstExecution = true;

        [TestInitialize()]
        public void setUp()
        {
            if (firstExecution)
            {
                string key_sandbox = System.IO.File.ReadAllText(@"\\psf\Home\Documents\postmen_sandbox_key.txt");
                string key_testing = System.IO.File.ReadAllText(@"\\psf\Home\Documents\postmen_testing_key.txt");
                string key_production_sandbox = System.IO.File.ReadAllText(@"\\psf\Home\Documents\postmen_production_sandbox_key.txt");
                Console.WriteLine("Key sandbox: " + key_sandbox);
                handler_production_sandbox = new HandlerAPI(api_key: key_production_sandbox, endpoint: endpoint_production_sandbox);

                address1 = JObject.Parse(@"{
                        city: 'Los Angeles',
                        state: 'CA',
                        postal_code: '90001',
                        country: 'USA'
                   }");

                address2 = JObject.Parse(@"{
                        city:'Beverly Hills',
                        postal_code:'90209',
                        state:'CA',
                        country:'USA'
                    }");

                address_wrong_address = JObject.Parse(@"{
                        city:'Beverly Hills',
                        postal_code:'90209',
                        state:'CA',
                        country:'AAA'
                    }");

                parcels1 = JArray.Parse(@"[
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

				JObject shipper_account_element = new JObject();
				string account_dhl_production = System.IO.File.ReadAllText(@"\\psf\Home\Documents\postmen_production_sandbox_account_key_dhl.txt"); 
				shipper_account_element.Add("id", account_dhl_production);
				shipper_account_production_sandbox_array = new JArray();
				shipper_account_production_sandbox_array.Add(shipper_account_element);

				shipper_account_production_sandbox = new JObject();
				shipper_account_production_sandbox.Add("id", account_dhl_production);
            }


        }

        [TestMethod]
        public void RatesFakeAPI()
        {
            HandlerAPI wrong_handler = new HandlerAPI(api_key: "123456789876543456", endpoint: endpoint_production_sandbox);
            //if the trackingNumber is bad informed
            dynamic request_data = new JObject();

            request_data.shipper_accounts = shipper_account_production_sandbox_array;
            request_data.async = false;
            request_data.shipment = new JObject();
            request_data.shipment.ship_from = address1;
            request_data.shipment.ship_to = address2;
            request_data.shipment.parcels = parcels1;

            try
            {
                wrong_handler.create(resource: "rates", payload: request_data);
                //shouldn't reach this point
                Assert.Equals(0, 1);
            }
            catch (PostmenException e)
            {
                Console.WriteLine(request_data.ToString());
                Console.WriteLine("--------------------");
                Console.WriteLine(e.Raw.ToString());
                Assert.AreEqual(4105, e.Code);
            }
        }

        [TestMethod]
        public void RatesWrongCountry()
        {
            dynamic request_data = new JObject();
            request_data.shipper_accounts = shipper_account_production_sandbox_array;
            request_data.async = false;
            request_data.shipment = new JObject();
            request_data.shipment.ship_from = address1;
            request_data.shipment.ship_to = address_wrong_address;
            request_data.shipment.parcels = parcels1;

            try
            { 
                handler_production_sandbox.create(resource: "rates", payload: request_data);
                //shouldn't reach this point
                Assert.Equals(0, 1);
            }
            catch (PostmenException e)
            {
                Console.WriteLine(request_data.ToString());
                Console.WriteLine("--------------------");
                Console.WriteLine(e.Raw.ToString());
                Assert.AreEqual(4104, e.Code);
            }
        }

        [TestMethod]
        public void RatesAsynchronous()
        {
                dynamic request_data = new JObject();
                request_data.shipper_accounts = shipper_account_production_sandbox_array;
                request_data.async = true;
                request_data.shipment = new JObject();
                request_data.shipment.ship_from = address1;
                request_data.shipment.ship_to = address2;
                request_data.shipment.parcels = parcels1;

                JObject result = handler_production_sandbox.create(resource: "rates", payload: request_data);
                Console.WriteLine(request_data.ToString());
                Console.WriteLine("--------------------");
                Console.WriteLine(result.ToString());
                Assert.AreEqual(result["data"]["status"], "calculating");
        }
        
        [TestMethod]
        public void RatesOk()
        {
                dynamic request_data = new JObject();
                request_data.shipper_accounts = shipper_account_production_sandbox_array;
                request_data.async = false;
                request_data.shipment = new JObject();
                request_data.shipment.ship_from = address1;
                request_data.shipment.ship_to = address2;
                request_data.shipment.parcels = parcels1;

                JObject result = handler_production_sandbox.create(resource: "rates", payload: request_data);
                Console.WriteLine(request_data.ToString());
                Console.WriteLine("--------------------");
                Console.WriteLine(result.ToString());
                Assert.AreEqual(result["data"]["status"], "calculated");
           
        }

        [TestMethod]
        public void LabelWrongAddress()
        {
            DateTime localDate = DateTime.Now;
            dynamic request_data = new JObject();
            request_data.shipper_account = shipper_account_production_sandbox;
            request_data.async = false;
            request_data.shipment = new JObject();
            request_data.shipment.ship_from = address1;
            request_data.shipment.ship_to = address_wrong_address;
            request_data.shipment.parcels = parcels1;
            request_data.ship_date = localDate.ToString("yyyy-MM-dd");
            request_data.service_type = "dhl_express_0900";

            request_data.shipment.ship_from.street1 = "example street1";
            request_data.shipment.ship_from.contact_name = "contact_name1";
            request_data.shipment.ship_from.phone = "123456789";
            request_data.shipment.ship_to.street1 = "example street1";
            request_data.shipment.ship_to.contact_name = "contanct name1";
            request_data.shipment.ship_to.phone = "123456789";

            request_data.billing = JObject.Parse(@"{paid_by: 'shipper', method: { account_number: '950000002',type: 'account'}}");

            try
            {
                handler_production_sandbox.create(resource: "labels", payload: request_data);
                //shouldn't reach this point
                Assert.Equals(0, 1);
            }
            catch (PostmenException e)
            {
                Console.WriteLine(request_data.ToString());
                Console.WriteLine("--------------------");
                Console.WriteLine(e.Raw.ToString());
                Assert.AreEqual(4104, e.Code);
            }

        }

        [TestMethod]
        public void LabelAsync()
        {
            DateTime localDate = DateTime.Now;
            dynamic request_data = new JObject();
            request_data.shipper_account = shipper_account_production_sandbox;
            request_data.async = true;
            request_data.shipment = new JObject();
            request_data.shipment.ship_from = address1;
            request_data.shipment.ship_to = address2;
            request_data.shipment.parcels = parcels1;
            request_data.ship_date = localDate.ToString("yyyy-MM-dd");
            request_data.service_type = "dhl_express_0900";

            request_data.shipment.ship_from.street1 = "example street1";
            request_data.shipment.ship_from.contact_name = "contact_name1";
            request_data.shipment.ship_from.phone = "123456789";
            request_data.shipment.ship_to.street1 = "example street1";
            request_data.shipment.ship_to.contact_name = "contanct name1";
            request_data.shipment.ship_to.phone = "123456789";

            request_data.billing = JObject.Parse(@"{paid_by: 'shipper', method: { account_number: '950000002',type: 'account'}}");

            Console.WriteLine(request_data.ToString());
            JObject result = handler_production_sandbox.create(resource: "labels", payload: request_data);
            Console.WriteLine("--------------------");
            Console.WriteLine(result.ToString());
            Assert.AreEqual(result["data"]["status"], "creating");

        }

        [TestMethod]
        public void LabelOk()
        {
                DateTime localDate = DateTime.Now;
                dynamic request_data = new JObject();
                request_data.shipper_account = shipper_account_production_sandbox;
                request_data.async = false;
                request_data.shipment = new JObject();
                request_data.shipment.ship_from = address1;
                request_data.shipment.ship_to = address2;
                request_data.shipment.parcels = parcels1;
                request_data.ship_date = localDate.ToString("yyyy-MM-dd");
                request_data.service_type = "dhl_express_0900";

                request_data.shipment.ship_from.street1 = "example street1";
                request_data.shipment.ship_from.contact_name = "contact_name1";
                request_data.shipment.ship_from.phone = "123456789";
                request_data.shipment.ship_to.street1 = "example street1";
                request_data.shipment.ship_to.contact_name = "contanct name1";
                request_data.shipment.ship_to.phone = "123456789";

                request_data.billing = JObject.Parse(@"{paid_by: 'shipper', method: { account_number: '950000002',type: 'account'}}");

                Console.WriteLine(request_data.ToString());
                JObject result = handler_production_sandbox.create(resource: "labels", payload: request_data);
                Console.WriteLine("--------------------");
                Console.WriteLine(result.ToString());
                Assert.AreEqual(result["data"]["status"], "created");
           
        }
    }
}
