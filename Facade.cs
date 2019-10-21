using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Assign_3
{
    public class Facade
    {
        private List<Category> categories = new List<Category>
        {
            new Category {cid = 1, name = "Beverages"},
            new Category {cid = 2, name = "Condiments"},
            new Category {cid = 3, name = "Confections"}
        };
        
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