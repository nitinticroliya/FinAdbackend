using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

//using System.Web.Script.Serialization;

namespace FinAd.Controllers
{
    [ApiController]
    [Route("/api")]

    

    public class FinAd : ControllerBase
    {

        private IConfiguration _config;
        public FinAd(IConfiguration config)
        {
            _config = config;
        }


        private UserModel GetCurrentUser()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (identity != null)
            {
                var userClaims = identity.Claims;

                return new UserModel
                {
                     Email = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Email)?.Value,
                    
                };
            }
            return null;
        }

        // Adding advisors info into Advisor table
        [HttpPost("/advisorSignup")]
        public IActionResult PostClient(User user)
        {
            /*var advisorList = new List<advisorSignup>();*/
            Console.WriteLine(user.Email + " " + user.Password);

            String connnectionString = "Data Source=L-3Q8PHR3\\SQLEXPRESS;Initial Catalog=FinAdDatabase;Integrated Security=True";
            using (SqlConnection connection = new(connnectionString))
            {
                    try
                    {
                        String sql = "INSERT INTO advisorSignup (Name, PhoneNo, Email, Address, Password) " +
                        "VALUES('"+ user.Name + "', '"+ user.PhoneNo + "', '"+ user.Email + "', '"+ user.Address + "', '"+user.Password+"')";
                        connection.Open();
                        using (SqlCommand command = new(sql, connection))
                        {
                        {
                            Console.WriteLine(sql);
                            command.ExecuteNonQuery();
                            Console.WriteLine("string");
                        }
                    }
                            

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);

                    }

                

                
            }
            return Ok("done");
        }



        //Checking if the email and password matches in the Advisor table and login
        [HttpPost("/advisorLogin")]

        public IActionResult AdvisorData(Login login)
        {
            var advisor = Authenticate(login);
            if (advisor != null)
            {
                var token = Generate(advisor);
                var tokken = token?.GetType().GetProperty("Value")?.GetValue(token, null);
                HttpContext.Response.Headers.Add("token", tokken.ToString());
                /*Console.WriteLine(JsonConvert.SerializeObject(token));*/
                return Ok(true);
            }
            else
            {
                return NotFound("User not found");
            }

        }

        public Object Generate(AdvisorData advisor)
        {
            Console.WriteLine(advisor.Email);
            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_config["Jwt:Key"]));
            var Credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Email, advisor.Email)

            };

            var token = new JwtSecurityToken(_config["Jwt:Issuer"], 
                _config["Jwt:Audience"],
                claims,
                DateTime.UtcNow,
                DateTime.Now.AddMinutes(50),
                Credentials);
            var obj = new JwtSecurityTokenHandler().WriteToken(token);
            return Ok(obj);
        }

        public AdvisorData Authenticate(Login login)
        {
            AdvisorData user =null;
            /*var advisorList = new List<advisorSignup>();*/
            Console.WriteLine(login.Email + " " + login.Password);


            String connnectionString = "Data Source=L-3Q8PHR3\\SQLEXPRESS;Initial Catalog=FinAdDatabase;Integrated Security=True";
            using (SqlConnection connection = new(connnectionString))
            
            {
                try
                {
                   string sql = "SELECT Email, Password FROM advisorSignup WHERE Email = '" + login.Email + "' AND Password = '" + login.Password + "'";
                    SqlCommand command = new SqlCommand(sql, connection);
                    connection.Open();
                    SqlDataReader rdr = command.ExecuteReader();
                    while (rdr.Read())
                    {
                        AdvisorData usr = new AdvisorData();
                        usr.Email = rdr.GetString(0);
                        user = usr;
                        
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    connection.Close();
                    return user;

                }
            }
            return user;
        }



        //returning model list according to risk profile of client
        [Authorize]
        [HttpPost("/desiredModelList")]
        public OkObjectResult GetDesiredModels(GetModelList models)
        {
            var advisor = GetCurrentUser();
            List<GetModelList> modelsList = new List<GetModelList>();

            String connnectionString = "Data Source=L-3Q8PHR3\\SQLEXPRESS;Initial Catalog=FinAdDatabase;Integrated Security=True";

            using (SqlConnection connection = new(connnectionString))
            {
                try
                {
                    String sql = "SELECT DISTINCT ModelName FROM  Models WHERE AdvisorEmail = '" + advisor.Email +"' AND RiskProfile = '" + models.RiskProfile +"' ";
                    SqlCommand command = new SqlCommand(sql, connection);
                    connection.Open();
                    SqlDataReader rdr = command.ExecuteReader();
                    while (rdr.Read())
                    {
                        GetModelList listModels = new GetModelList();
                        listModels.Model = rdr["ModelName"].ToString();
                        /*listModels.RiskProfile = rdr["RiskProfile"].ToString();*/
                        modelsList.Add(listModels);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);

                }

            }
            //  Context.Response.Write(js.Serialize(ExistingClientsData));
            var desiredModelInfo = JsonConvert.SerializeObject(modelsList);
            return Ok(desiredModelInfo);
        }



        // Adding new client data into clients Table
        [Authorize]
        [HttpPost("/newClientData")]
        public IActionResult PostClient( newClientsData newClient)
        {
            /*var advisorList = new List<advisorSignup>();*/
            Console.WriteLine(newClient.Email + " " + newClient.RiskProfile);
            var advisor = GetCurrentUser();

            String connnectionString = "Data Source=L-3Q8PHR3\\SQLEXPRESS;Initial Catalog=FinAdDatabase;Integrated Security=True";
            using (SqlConnection connection = new(connnectionString))
            {
                try
                {
                    String sql = "INSERT INTO clientsData (Name, PhoneNo, Email, PancardNo, AadharNo, RiskProfile, SuggestedModel, AdvisorEmail) " +
                    "VALUES('" + newClient.Name + "', '" + newClient.PhoneNo + "', '" + newClient.Email + "', '" + newClient.PancardNo + "', '" + newClient.AadharNo + "', '" + newClient.RiskProfile + "', '" + newClient.SuggestedModel + "', '" + advisor.Email + "')";
                    connection.Open();  
                    using (SqlCommand command = new(sql, connection))
                    {
                        {
                            Console.WriteLine(sql);
                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return NotFound("Not Operated");
                }
            }
            return Ok("Client Added Successfully");
        }


        // Fetching list of all clients
        [Authorize]
        [HttpGet("/existingClientsList")]
        public OkObjectResult GetClientList()
        {
            var advisor = GetCurrentUser();
            List<ClientList> clientList = new List<ClientList>();

            String connnectionString = "Data Source=L-3Q8PHR3\\SQLEXPRESS;Initial Catalog=FinAdDatabase;Integrated Security=True";

            using (SqlConnection connection = new(connnectionString))
            {
                try
                {
                    String sql = "SELECT DISTINCT(Email) FROM  clientsData WHERE AdvisorEmail = '" + advisor.Email + "'";
                    SqlCommand command = new SqlCommand(sql, connection);
                    connection.Open();
                    SqlDataReader rdr = command.ExecuteReader();
                    while (rdr.Read())
                    {
                        Console.WriteLine("gcjfxd");
                        ClientList client = new ClientList();
                        client.ClientEmail= rdr["Email"].ToString();
                        Console.WriteLine(client.ClientEmail);
                        clientList.Add(client);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);

                }

            }
            var clientInfo = JsonConvert.SerializeObject(clientList);
            return Ok(clientInfo);
        }


        // Fetching all clients details
        [Authorize]
        [HttpPost("/existingClientsData")]
        public IActionResult GetAllClients(ClientEmail c)
        {
            var advisor = GetCurrentUser();
            List<ExistingClientsData> clientList = new List<ExistingClientsData>();

            String connnectionString = "Data Source=L-3Q8PHR3\\SQLEXPRESS;Initial Catalog=FinAdDatabase;Integrated Security=True";

            using (SqlConnection connection = new(connnectionString))
            {
                try
                {
                    String sql = "SELECT * FROM  clientsData WHERE AdvisorEmail = '" + advisor.Email +"' AND Email = '" + c.EmailId +"'";
                    SqlCommand command = new SqlCommand(sql, connection);
                    connection.Open();
                    SqlDataReader rdr = command.ExecuteReader();
                    while (rdr.Read())
                    {
                        Console.WriteLine("gcjfxd");
                        ExistingClientsData client = new ExistingClientsData();
                        client.Name = rdr["Name"].ToString();
                        client.Email = rdr["Email"].ToString();
                        client.PhoneNo = (int?)Convert.ToInt64(rdr["PhoneNo"]);
                        client.PancardNo = rdr["PancardNo"].ToString();
                        client.AadharNo = (int?)Convert.ToInt64(rdr["AadharNo"]);
                        client.RiskProfile = rdr["RiskProfile"].ToString();
                        client.SuggestedModel = rdr["SuggestedModel"].ToString();
                        Console.WriteLine(client.Email);
                        clientList.Add(client);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return NotFound("NotFound");

                }

            }
            var clientInfo = JsonConvert.SerializeObject(clientList);
            return Ok(clientInfo);
        }


        // delete the client
        [Authorize]
        [HttpPost("/deleteClientsData")]
        public IActionResult DeleteClient(ClientDelete client)
        {
            var advisor = GetCurrentUser();
            try
            {
                string Email = Request.Query["email"].ToString();
                String connnectionString = "Data Source=L-3Q8PHR3\\SQLEXPRESS;Initial Catalog=FinAdDatabase;Integrated Security=True";
                using (SqlConnection connection = new(connnectionString))
                {
                    connection.Open();
                    String sql = "DELETE FROM  clientsData WHERE Email = '" + client.Email + "' AND AdvisorEmail = '" + advisor.Email + "'";
                    Console.WriteLine("abc");
                    SqlCommand command = new SqlCommand(sql, connection);
                    {
                        Console.WriteLine("xyz");
                        //command.Parameters.AddWithValue(client.Email, Email);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(ex.Message);
            }
            return Ok("client is deleted");
        }



        [HttpGet("/securitiesList")]
        public OkObjectResult GetAllSecurities()
        {
            List<SecuritiesList> securityList = new List<SecuritiesList>();

            String connnectionString = "Data Source=L-3Q8PHR3\\SQLEXPRESS;Initial Catalog=FinAdDatabase;Integrated Security=True";

            using (SqlConnection connection = new(connnectionString))
            {
                try
                {
                    String sql = "SELECT * FROM  listOfSecurities ";
                    SqlCommand command = new SqlCommand(sql, connection);
                    connection.Open();
                    SqlDataReader rdr = command.ExecuteReader();
                    while (rdr.Read())
                    {
                        SecuritiesList security = new SecuritiesList();
                        security.AssetClass = rdr["Asset_Class"].ToString();
                        security.Securities = rdr["Security_Name"].ToString();
                        securityList.Add(security);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);

                }

            }
            var securityInfo = JsonConvert.SerializeObject(securityList);
            return Ok(securityInfo);
        }


        /*[HttpGet("/selectedSecuritiesList")]
        public OkObjectResult GetSelectecSecurities()
        {
            List<SecuritiesList> securityList = new List<SecuritiesList>();

            String connnectionString = "Data Source=L-3Q8PHR3\\SQLEXPRESS;Initial Catalog=FinAdDatabase;Integrated Security=True";

            using (SqlConnection connection = new(connnectionString))
            {
                try
                {
                    String sql = "SELECT * FROM  listOfSecurities ";
                    SqlCommand command = new SqlCommand(sql, connection);
                    connection.Open();
                    SqlDataReader rdr = command.ExecuteReader();
                    while (rdr.Read())
                    {
                        SecuritiesList security = new SecuritiesList();
                        security.Securities = rdr["Security_Name"].ToString();

                        securityList.Add(security);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);

                }

            }

            // JavaScriptSerializer js = new JavaScriptSerializer();
            //  Context.Response.Write(js.Serialize(ExistingClientsData));
            var securityInfo = JsonConvert.SerializeObject(securityList);
            return Ok(securityInfo);
        }
*/


        // adding model securities and weightage to datatbase
        [Authorize]
        [HttpPost("/newModelData")]
        public IActionResult ModelData(NewModelData newModel)
        {
            var advisor = GetCurrentUser();

            String connnectionString = "Data Source=L-3Q8PHR3\\SQLEXPRESS;Initial Catalog=FinAdDatabase;Integrated Security=True";
            using (SqlConnection connection = new(connnectionString))
            {
                try
                {
                    String sql = "INSERT INTO Models (AdvisorEmail, RiskProfile, ModelName, AssetClass, Securities, Weightage) " +
                    "VALUES('" + advisor.Email + "', '" + newModel.RiskProfile + "', '" + newModel.ModelName + "', '" + newModel.AssetClass + "', '" + newModel.Securities + "', '" + newModel.Weightage + "')";
                    connection.Open();
                    using (SqlCommand command = new(sql, connection))
                    {
                        {
                            Console.WriteLine(sql);
                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);

                }
            }
            return Ok("Model Added Successfully");
        }

        // returning RiskProfiles List
        [Authorize]
        [HttpGet("/RiskProfileList")]
        public OkObjectResult GetDesiredRiskProfile()
        {
            var advisor = GetCurrentUser();

            List<GetModelList> riskProfileList = new List<GetModelList>();

            String connnectionString = "Data Source=L-3Q8PHR3\\SQLEXPRESS;Initial Catalog=FinAdDatabase;Integrated Security=True";

            using (SqlConnection connection = new(connnectionString))
            {
                try
                {
                    Console.WriteLine("abc");
                    String sql = "SELECT DISTINCT RiskProfile FROM  Models WHERE AdvisorEmail = '" + advisor.Email + "' ";
                    SqlCommand command = new SqlCommand(sql, connection);
                    connection.Open();
                    SqlDataReader rdr = command.ExecuteReader();
                    while (rdr.Read())
                    {
                        Console.WriteLine("xyz");
                        GetModelList listRiskProfiles = new GetModelList();
                        listRiskProfiles.RiskProfile = rdr["RiskProfile"].ToString();
                        riskProfileList.Add(listRiskProfiles);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            var riskProfileInfo = JsonConvert.SerializeObject(riskProfileList);
            return Ok(riskProfileInfo);
        }



        // Fetching All model name for selected risk Profile
        [Authorize]
        [HttpPost("/selectedModelList")]
        public OkObjectResult GetModelslist(GetModelList models)
        {
            var advisor = GetCurrentUser()  ;
            List<GetModelList> modelsList = new List<GetModelList>();

            String connnectionString = "Data Source=L-3Q8PHR3\\SQLEXPRESS;Initial Catalog=FinAdDatabase;Integrated Security=True";

            using (SqlConnection connection = new(connnectionString))
            {
                try
                {
                    String sql = "SELECT DISTINCT ModelName FROM  Models WHERE AdvisorEmail = '" + advisor.Email + "' AND RiskProfile = '" + models.RiskProfile + "' ";
                    SqlCommand command = new SqlCommand(sql, connection);
                    connection.Open();
                    SqlDataReader rdr = command.ExecuteReader();
                    while (rdr.Read())
                    {
                        GetModelList listModels = new GetModelList();
                        listModels.Model = rdr["ModelName"].ToString();
                        /*listModels.RiskProfile = rdr["RiskProfile"].ToString();*/
                        modelsList.Add(listModels);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            var desiredModelInfo = JsonConvert.SerializeObject(modelsList);
            return Ok(desiredModelInfo);
        }





        // Fetching all models
        [Authorize]
        [HttpPost("/existingModelsData")]
        public OkObjectResult GetAllModels(GetModelList modelinfo)
        {
            List<ExistingModelsData> modelsList = new List<ExistingModelsData>();
            var advisor = GetCurrentUser();

            String connnectionString = "Data Source=L-3Q8PHR3\\SQLEXPRESS;Initial Catalog=FinAdDatabase;Integrated Security=True";

            using (SqlConnection connection = new(connnectionString))
            {
                try
                {
                    String sql = "SELECT AssetClass, Securities, Weightage FROM Models WHERE AdvisorEmail = '" + advisor.Email + "' AND RiskProfile = '" + modelinfo.RiskProfile + "' AND ModelName = '" + modelinfo.Model + "' ";
                    SqlCommand command = new SqlCommand(sql, connection);
                    connection.Open();
                    SqlDataReader rdr = command.ExecuteReader();

                    Console.WriteLine("jhvuv");
                    while (rdr.Read())
                    {
                        ExistingModelsData models = new ExistingModelsData();
                        models.Asset = rdr["AssetClass"].ToString();
                        models.Securities = rdr["Securities"].ToString();
                        models.Weightage = Convert.ToInt32(rdr["Weightage"]);
                        modelsList.Add(models);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);

                }

            }
            var modelInfo = JsonConvert.SerializeObject(modelsList);
            return Ok(modelInfo);
        }



        // Fetching Models Details for making PieChart
        [Authorize]
        [HttpPost("/modelPieChart")]
        public IActionResult GetModelPieChart(GetModelList modelinfo)
        {
            List<ModelPieChart> modelsList = new List<ModelPieChart>();
            var advisor = GetCurrentUser();

            String connnectionString = "Data Source=L-3Q8PHR3\\SQLEXPRESS;Initial Catalog=FinAdDatabase;Integrated Security=True";

            using (SqlConnection connection = new(connnectionString))
            {
                try
                {
                    String sql = "SELECT DISTINCT(AssetClass), (SELECT SUM(Weightage) FROM Models B WHERE B.AssetClass=A.AssetClass AND AdvisorEmail = '" + advisor.Email + "' AND RiskProfile = '" + modelinfo.RiskProfile + "' AND ModelName = '" + modelinfo.Model + "') AS AssetSum FROM Models A WHERE (SELECT SUM(Weightage) FROM Models B WHERE B.AssetClass=A.AssetClass AND AdvisorEmail = '" + advisor.Email + "' AND RiskProfile = '" + modelinfo.RiskProfile + "' AND ModelName = '" + modelinfo.Model + "') IS NOT NULL";
                    SqlCommand command = new SqlCommand(sql, connection);
                    connection.Open();
                    SqlDataReader rdr = command.ExecuteReader();

                    Console.WriteLine("jhvuv");
                    Console.WriteLine(sql);
                    while (rdr.Read())
                    {
                        ModelPieChart models = new ModelPieChart();
                       // Console.WriteLine(+ "->" +);
                        models.AssetType = rdr.GetString(0);
                        models.AssetSum = rdr.GetInt32(1);
                        
                        modelsList.Add(models);
                    }
                }
                catch (Exception ex)
                {

                    Console.WriteLine(ex.Message);
                    return NotFound(ex.Message);

                }

            }
            var modelInfo = JsonConvert.SerializeObject(modelsList);
            return Ok(modelInfo);
        }


        // fetching all the questions to admin static data
        [HttpGet("/allQuestions")]
        public OkObjectResult GetAllQuestions(QuestionsData question)
        {
            List<QuestionsData> questionList = new List<QuestionsData>();

            String connnectionString = "Data Source=L-3Q8PHR3\\SQLEXPRESS;Initial Catalog=FinAdDatabase;Integrated Security=True";

            using (SqlConnection connection = new(connnectionString))
            {
                try
                {
                    String sql = "SELECT ID, Questions FROM  staticQuestionsData ";
                    SqlCommand command = new SqlCommand(sql, connection);
                    connection.Open();
                    SqlDataReader rdr = command.ExecuteReader();
                    while (rdr.Read())
                    {
                        QuestionsData ques = new QuestionsData();
                        ques.ID = rdr["ID"].ToString();
                        ques.Questions = rdr["Questions"].ToString();
                        questionList.Add(ques);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);

                }

            }

            var quesinfo = JsonConvert.SerializeObject(questionList);
            return Ok(quesinfo);
        }
    }
}
