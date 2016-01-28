using System;
using System.Collections.Generic; using System.Linq;
using System.Security.Authentication; using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using SAL.Data.Entities;
using SAL.Helpers;
using SAL.Models;
using SAL.Models.Converters;
using SAL.QueryService;
using SAL.SupplierService;
using SAL.UserService;

namespace SAL.Controllers
{
    [Authorize]
    public class QueryController : Controller
    {
        QueryServiceClient service = new QueryServiceClient();
        SupplierServiceClient supplierService = new SupplierServiceClient();
        UserServiceClient userService = new UserServiceClient();
        List<StatusModel> statuses = new List<StatusModel>();
        List<SupplierModel> suppliers = new List<SupplierModel>();
        List<CategoryModel> categories = new List<CategoryModel>();
        //
        // GET: /Query/
        public ActionResult Index()
        {
            PopulateUsers();
            
            
            
            PopulateSuppliers();
            PopulateStatus();
            PopulatePriority();
            PopulateCategory();
            var user = userService.GetUserProfile(User.Identity.Name);
            if (user == null)
            throw new InvalidCredentialException("User has no permissions for this operation");
            ViewData["IsSupplierAdmin"] = IsSupplierAdmin(user.Name);
            ViewData["Count"] = service.GetSupplierOpenQueries(user.SupplierId).Count();
            return View();
        }
        public ActionResult List([DataSourceRequest]DataSourceRequest request)
        {
            PopulateSuppliers();
            PopulateStatus();
            PopulatePriority();
            PopulateUsers();
            PopulateCategory();
            var user = userService.GetUserProfile(User.Identity.Name);
            if (user == null)
            throw new InvalidCredentialException("User has no permissions for this operation");
            ViewData["IsSupplierAdmin"] = IsSupplierAdmin(user.Name);
            var data = service.GetSupplierQueries(user.SupplierId).Select(QueryConverter.Convert);
            return Json(data.ToDataSourceResult(request));
        }
        public ActionResult Suppliers()
        {
            PopulateSuppliers();
            PopulateStatus();
            PopulatePriority();
            
            PopulateUsers();
            PopulateCategory();
            return View();
        }
        public ActionResult GetSuppliers([DataSourceRequest]DataSourceRequest request)
        {
            PopulateSuppliers();
            PopulateStatus();
            PopulatePriority();
            PopulateUsers();
            PopulateCategory();
            //TODO: split by role (Supplier / LA Staff)
            var data =
            service.GetAllQueries().Select(QueryConverter.Convert).OrderByDescending(x=>x.QueryDate).ThenByDescending(x=>x.StatusId);
            return Json(data.ToDataSourceResult(request));
        }
        //
        // GET: /Query/Create
        [HttpPost]
        public ActionResult Update([DataSourceRequest] DataSourceRequest request, QueryModel model)
        {
            try
            {
                PopulatePriority();
                PopulateSuppliers();
                PopulateStatus();
                PopulateUsers();
                PopulateCategory();
                if (model!= null && !(model.StatusId > 0))
                ModelState.AddModelError("StatusId","Status field required.");
                
                
                
                
                if (model != null && ModelState.IsValid)
                {
                    var user = userService.GetUserProfile(User.Identity.Name);
                    if (user == null)
                    throw new InvalidCredentialException("User has no permission for this operation");
                    model.Query = HttpUtility.HtmlDecode(model.Query);
                    model.RaisedById = user.UserId;
                    model.Supplier = new SupplierModel() { Id = user.SupplierId };
                    service.UpdateQuery(QueryConverter.ConvertBack(model));
                    var data = service.GetSupplierQueries(user.SupplierId).Select(QueryConverter.Convert);
                    return Json(data.ToDataSourceResult(request));
                }
                
                return Json(ModelState.ToDataSourceResult());
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.ToString());
            }
        }
//
// GET: /Query/Create
        [HttpPost]
        public ActionResult Create([DataSourceRequest] DataSourceRequest request, QueryModel model)
        {
            try
            {
                PopulatePriority();
                PopulateSuppliers();
                PopulateStatus();
                PopulateUsers();
                PopulateCategory();
                
                if (model != null && ModelState.IsValid)
                {
                    var user = userService.GetUserProfile(User.Identity.Name);
                    if (user == null)
                    throw new InvalidCredentialException("User has no permission for this operation");
                    model.Query = HttpUtility.HtmlDecode(model.Query);
                    model.RaisedById = user.UserId;
                    model.PriorityId = "Normal";
                    model.Supplier = new SupplierModel() {Id = user.SupplierId};
                    service.CreateQuery(QueryConverter.ConvertBack(model));
                    var data = service.GetSupplierQueries(user.SupplierId).Select(QueryConverter.Convert);
                    return Json(data.ToDataSourceResult(request));
                }
                return Json(ModelState.ToDataSourceResult());
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.ToString());
            }
        }
        [HttpPost]
        public ActionResult Reply(int taskId, string reply)
        {
            var user = userService.GetUserProfile(User.Identity.Name);
            if (user == null)
            throw new InvalidCredentialException("User has no permission for this operation");
            var queryThread = new QueryThreadEntity()
            {
                Reply = HttpUtility.HtmlDecode(reply),
                ReplyDate = DateTime.UtcNow,
                TaskId = taskId,
                
                
                RaisedById = user.UserId
            };
            var result = service.CreateQueryThread(taskId, queryThread);
            if (result)
            return Json(new {IsSuccess = true});
            return Json(new {IsSuccess = false, Message = "Reply cannot be created."});
        }
        [HttpPost]
        public ActionResult Close(int taskId)
        {
            string userName = string.Empty;
            var user = userService.GetUserProfile(User.Identity.Name);
            if (user == null)
            throw new InvalidCredentialException("User has no permission for this operation");
            if (User.IsInRole("Account Staff"))
            {
                userName = user.Name;
            }
            var result =  service.CloseThread(taskId,userName);
            if (result)
            return Json(new {IsSuccess = true});
            return Json(new { IsSuccess = false, Message = "You cannot close the thread." });
        }
        public ActionResult Edit(int taskId)
        {
            var user = userService.GetUserProfile(User.Identity.Name);
            if (user == null)
            throw new InvalidCredentialException("User has no permission for this operation");
            var thread = service.GetSupplierQueryThreads(taskId, user.SupplierId).Select(QueryConverter.Convert);
            var model = new QueryViewModel()
            
            
            {
                QueryThread = thread,
                TaskId = taskId
            };
            return PartialView("_QueryThreadPartial", model);
        }
        public ActionResult EditSupplier(int taskId, string suppId)
        {
            var thread = service.GetSupplierQueryThreads(taskId, suppId).Select(QueryConverter.Convert);
            var model = new QueryViewModel()
            {
                QueryThread = thread,
                TaskId = taskId
            };
            return PartialView("_QueryListPartial", model);
        }
        [HttpPost]
        public ActionResult Delete([DataSourceRequest] DataSourceRequest request,QueryModel model)
        {
            var user = userService.GetUserProfile(User.Identity.Name);
            if (user != null)
            {
                service.DeleteQuery(QueryConverter.ConvertBack(model));
            }
            return Json(ModelState.ToDataSourceResult());
        }
        public ActionResult Statuses()
        {
            var result = service.GetQueryStatuses().Select(s => new StatusModel() { Status = s.Name, Value = s.Status, Id = s.Status });
            
            
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetUsers()
        {
            var users = userService.GetUsersByRole("Account Staff").Select(u => new UserModel() { UserId = u.UserId, UserName = u.Name });
            return Json(users, JsonRequestBehavior.AllowGet);
        }
        private void PopulatePriority()
        {
            var priorities = new List<PriorityModel>()
            {
                new PriorityModel() {Value = "Normal"},
                new PriorityModel() {Value = "High"},
                new PriorityModel() {Value = "Low"}
            };
            ViewData["Priorities"] = priorities;
            ViewData["defaultPriority"] = "Normal";
        }
        private void PopulateUsers()
        {
            var users = userService.GetUsersByRole("Account Staff").Select(u => new UserModel() { UserId = u.UserId, UserName =
                u.Name }).ToList();
            users.Add(new UserModel(){ UserId = -1, UserName = "Empty"});
            ViewData["Users"] = users;
        }
        private void PopulateStatus()
        {
            statuses = service.GetQueryStatuses().Select(s => new StatusModel() { Status = s.Name, Value = s.Status, Id = s.Status }).ToList();
            ViewData["Statuses"] = statuses;
            
            
            
            
            ViewData["defaultStatus"] = statuses.FirstOrDefault();
        }
        private void PopulateCategory()
        {
            categories = service.GetCategories().Select(s => new CategoryModel() { CategoryId = s.CategoryId, CategoryDesc =
                s.CategoryDesc }).ToList();
            ViewData["Categories"] = categories;
        }
        private void PopulateSuppliers()
        {
            suppliers = supplierService.GetSupplierList().Select(s => new SupplierModel() { Id = s.Id, Name = s.Name, Email =
                s.Email }).ToList();
            var defaultSupplier = new SupplierModel() { Name = "Empty" };
            suppliers.Add(defaultSupplier);
            ViewData["Suppliers"] = suppliers;
            ViewData["defaultSupplier"] = defaultSupplier;
        }
        private bool IsSupplierAdmin(string userName)
        {
            var roles = userService.GetUserRoles(userName);
            if (roles != null && roles.Any(x => x.RoleName.Contains("Supplier Administrator")))
            return true;
            return false;
            } }
        }