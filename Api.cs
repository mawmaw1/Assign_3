using System;
using System.Collections.Generic;
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

        private List<Category> categories = new List<Category>
        {
            new Category {cid = 1, name = "Beverages"},
            new Category {cid = 2, name = "Condiments"},
            new Category {cid = 3, name = "Confections"}
        };

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
                if (!jobject.HasValues || jobject == null) return JsonConvert.SerializeObject(returnObj);

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
                if (jobject.ContainsKey("method") && jobject.ContainsKey("date") && jobject.ContainsKey("body"))
                {
                    switch (methodMatch.Value)
                    {
                        case "echo":
                            returnObj.status = statusCode[0];
                            returnObj.body = jobject.SelectToken("body");
                            return JsonConvert.SerializeObject(returnObj);
                    }

                    
                }
                
                // er path bra?
                Regex pathSpecific = new Regex(@"^\/api\/categories\/\d+$");
                Match pathMatchSpecific = pathSpecific.Match(jobject.SelectToken("path").ToString());
                Regex path = new Regex(@"^\/api\/categories$");
                Match pathMatch = path.Match(jobject.SelectToken("path").ToString());

                // read, delete
                if (jobject.ContainsKey("method") && jobject.ContainsKey("date") && jobject.ContainsKey("path"))
                {
                    switch (methodMatch.Value)
                    {
                        case "read":
                            if (pathMatch.Value.Length == 0 && pathMatchSpecific.Value.Length == 0)
                            {
                                returnObj.status = statusCode[3];
                                returnObj.body = null;
                            }
                            else
                            {
                                List<Category> returnCat = Read(jobject.SelectToken("path"));
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

                            break;

                        case "delete":
                            if (pathMatchSpecific.Value.Length == 0)
                            {
                                returnObj.status = statusCode[3];
                                returnObj.body = null;
                            }

                            Category c = Delete(jobject.SelectToken("path"));
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

                            break;
                    }
                }

                // update, create

                if (jobject.ContainsKey("method") && jobject.ContainsKey("date") && jobject.ContainsKey("path") &&
                    jobject.ContainsKey("body"))
                {
                    Regex body = new Regex(@"^[a-zA-Z]{1,25}$");
                    JToken jToken = jobject.SelectToken("body").ToString();
                    dynamic json = JsonConvert.DeserializeObject(jToken.ToString());
                    Match bodyMatch = body.Match(json["name"].ToString());

                    switch (methodMatch.Value)
                    {
                        case "create":
                            if (pathMatch.Value.Length == 0 || bodyMatch.Value.Length == 0)
                            {
                                returnObj.status = statusCode[3];
                                returnObj.body = null;
                                return JsonConvert.SerializeObject(returnObj);
                            }
                            returnObj.status = statusCode[1];
                            returnObj.body = JsonConvert.SerializeObject(Create(jobject.SelectToken("body")));

                            break;
                        case "update":
                            if (bodyMatch.Value.Length == 0 || pathMatchSpecific.Value.Length == 0)
                            {
                                returnObj.status = statusCode[3];
                                returnObj.body = null;
                            }

                            Category c = Update(jobject.SelectToken("path"), jobject.SelectToken("body"));
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

                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return JsonConvert.SerializeObject(returnObj);
        }

        public void Controller(string request)
        {
            
            var jobject = JsonConvert.DeserializeObject<JObject>(request);
            
            
        }


        public Category Create(JToken jToken)
        {
            int cid = categories.Count + 1;
            dynamic json = JsonConvert.DeserializeObject(jToken.ToString());

            categories.Add(new Category
            {
                cid = cid, name = json["name"]
            });
            return categories[categories.Count - 1];
        }

        public List<Category> Read(JToken jToken)
        {
            string path = jToken.ToString();
            string[] args = path.Split("/");
            if (args.Length != 4) return categories;
            else
            {
                List<Category> returnList = new List<Category>();
                int cid = 0;
                if (Int32.TryParse(args[3], out cid))
                {
                    Category c = categories.Find(category => category.cid == cid);
                    if (c != null) returnList.Add(c);
                }

                return returnList;
            }
        }

        public Category Update(JToken path, JToken update)
        {
            string convertedPath = path.ToString();
            string[] args = convertedPath.Split("/");

            dynamic json = JsonConvert.DeserializeObject(update.ToString());
            int cid = 0;
            if (Int32.TryParse(args[3], out cid))
            {
                Category c = categories.Find(category => category.cid == cid);
                if (c != null) c.name = json["name"];
                return c;
            }

            return null;
        }

        public Category Delete(JToken path)
        {
            string convertedPath = path.ToString();
            string[] args = convertedPath.Split("/");
            int cid = 0;
            if (Int32.TryParse(args[3], out cid))
            {
                Category c = categories.Find(category => category.cid == cid);
                if (c != null) categories.Remove(c);
                return c;
            }

            return null;
        }
    }
}