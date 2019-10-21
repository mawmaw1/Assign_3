using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Assign_3
{
    public class Api
    {
        public Api()
        {
        }
        
        Facade facade = new Facade();

        private List<string> statusCode = new List<string>
        {
            "1 Ok",
            "2 Created",
            "3 Updated",
            "4 Bad Request",
            "5 Not Found",
            "6 Error",
            "7 Missing Method, Illegal Method, Illegal Date, Missing date, missing resource, missing body, illegal body"
        };

        public string VerifyInput(string request)
        {
            dynamic returnObj = new JObject();
            returnObj.status = statusCode[6];
            returnObj.body = "";
            try
            {
                var jobject = JsonConvert.DeserializeObject<JObject>(request);
                if (!jobject.HasValues || jobject == null ) return JsonConvert.SerializeObject(returnObj);

                // Forkert dato eller metode
                Regex date = new Regex(@"\d{10}$");
                Match dateMatch = date.Match(jobject.SelectToken("date").ToString());
                Regex method = new Regex(@"^(create|read|update|delete|echo)$");
                Match methodMatch = method.Match(jobject.SelectToken("method").ToString());

                if (dateMatch.Value.Length == 0 || methodMatch.Value.Length == 0)
                {
                    returnObj.status = statusCode[6];
                    returnObj.body = "";
                    return JsonConvert.SerializeObject(returnObj);
                }
                //  echo
                if (jobject.ContainsKey("method") && jobject.ContainsKey("date") && jobject.ContainsKey("body") && methodMatch.Value.Equals("echo"))
                {
                    returnObj = Controller(jobject, methodMatch.Value);
                }
                
                // read, delete
                if (jobject.ContainsKey("method") && jobject.ContainsKey("date") && jobject.ContainsKey("path") && (methodMatch.Value.Equals("read")||methodMatch.Value.Equals("delete")))
                {
                    returnObj = Controller(jobject, methodMatch.Value);
                }
                // update, create
                if (jobject.ContainsKey("method") && jobject.ContainsKey("date") && jobject.ContainsKey("path") &&
                    jobject.ContainsKey("body")&& (methodMatch.Value.Equals("update")||methodMatch.Value.Equals("create")))
                {
                    returnObj = Controller(jobject, methodMatch.Value);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return JsonConvert.SerializeObject(returnObj);
        }

        public JObject Controller(JObject jobject, string value)
        {
            JObject returnObj = new JObject();
            if (value.Length != 0)
            {
                switch (value)
                {
                    case "create":
                        returnObj = CreateChecker(jobject);
                        break;
                    case "read":
                        returnObj = ReadChecker(jobject);
                        break;
                    case "update":
                        returnObj = UpdateChecker(jobject);
                        break;
                    case "delete":
                        returnObj = DeleteChecker(jobject);
                        break;
                    case "echo":
                        returnObj = EchoChecker(jobject);
                        break;
                }
            }

            return returnObj;
        }

        public JObject CreateChecker(JObject jobject)
        {
            dynamic returnObj = new JObject();
            Regex body = new Regex(@"^[a-zA-Z]{1,25}$");
            JToken jToken = jobject.SelectToken("body").ToString();
            dynamic json = JsonConvert.DeserializeObject(jToken.ToString());
            Match bodyMatch = body.Match(json["name"].ToString());
            Regex path = new Regex(@"^\/api\/categories$");
            Match pathMatch = path.Match(jobject.SelectToken("path").ToString());


            if (pathMatch.Value.Length == 0 || bodyMatch.Value.Length == 0)
            {
                returnObj.status = statusCode[3];
                returnObj.body = null;
                return returnObj;
            }

            returnObj.status = statusCode[1];
            returnObj.body = JsonConvert.SerializeObject(facade.Create(jobject.SelectToken("body")));
            return returnObj;
        }

        public JObject ReadChecker(JObject jobject)
        {
            dynamic returnObj = new JObject();
            Regex pathSpecific = new Regex(@"^\/api\/categories\/\d+$");
            Match pathMatchSpecific = pathSpecific.Match(jobject.SelectToken("path").ToString());
            Regex path = new Regex(@"^\/api\/categories$");
            Match pathMatch = path.Match(jobject.SelectToken("path").ToString());
            
            if (pathMatch.Value.Length == 0 && pathMatchSpecific.Value.Length == 0)
            {
                returnObj.status = statusCode[3];
                returnObj.body = null;
            }
            else
            {
                List<Category> returnCat = facade.Read(jobject.SelectToken("path"));
                returnObj.status = statusCode[0];
                switch (returnCat.Count)
                {
                    case 0:
                        returnObj.status = statusCode[4];
                        returnObj.body = null;
                        break;
                    case 1:
                        returnObj.body = JsonConvert.SerializeObject(returnCat[0]);
                        break;
                    default:
                        returnObj.body = JsonConvert.SerializeObject(returnCat);
                        break;
                }
            }

            return returnObj;
        }

        public JObject UpdateChecker(JObject jobject)
        {
            dynamic returnObj = new JObject();
            Regex body = new Regex(@"^[a-zA-Z]{1,25}$");
            JToken jToken = jobject.SelectToken("body").ToString();
            dynamic json = JsonConvert.DeserializeObject(jToken.ToString());
            Match bodyMatch = body.Match(json["name"].ToString());
            Regex pathSpecific = new Regex(@"^\/api\/categories\/\d+$");
            Match pathMatchSpecific = pathSpecific.Match(jobject.SelectToken("path").ToString());

            if (bodyMatch.Value.Length == 0 || pathMatchSpecific.Value.Length == 0)
            {
                returnObj.status = statusCode[3];
                returnObj.body = null;
            }
            else
            {
                Category c = facade.Update(jobject.SelectToken("path"), jobject.SelectToken("body"));
                if (c != null)

                {
                    returnObj.status = statusCode[2];
                    returnObj.body = null;
                }
                else
                {
                    returnObj.status = statusCode[4];
                    returnObj.body = "";
                }
            }
            return returnObj;
        }

        public JObject DeleteChecker(JObject jobject)
        {
            dynamic returnObj = new JObject();
            Regex pathSpecific = new Regex(@"^\/api\/categories\/\d+$");
            Match pathMatchSpecific = pathSpecific.Match(jobject.SelectToken("path").ToString());
            if (pathMatchSpecific.Value.Length == 0)
            {
                returnObj.status = statusCode[3];
                returnObj.body = null;
            }
            else
            {
                Category c = facade.Delete(jobject.SelectToken("path"));
                if (c != null)
                {
                    returnObj.status = statusCode[0];
                    returnObj.body = "";
                }
                else
                {
                    returnObj.status = statusCode[4];
                    returnObj.body = "";
                }
            }
            
            return returnObj;
        }

        public JObject EchoChecker(JObject jobject)
        {
            dynamic returnObj = new JObject();
            returnObj.status = statusCode[0];
            returnObj.body = jobject.SelectToken("body");
            return returnObj;

        }
        

    }
}