using TechWebSol.Data;
using TechWebSol.Models.DocumentModal;

namespace TechWebSol.DAL
{
    public class UploadFileDAL
    {
        private readonly ApplicationDbContext _context;
        public IConfiguration Configuration { get; }

        public UploadFileDAL(
            ApplicationDbContext context
            )
        {
            _context = context;
        }

        public UploadFileDAL(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            Configuration = configuration;
        }
        //public string GetFileFirstorDefault(Guid? Id)
        //{
        //    var documentEntity = _context.DocumentEntity.OrderByDescending(x => x.CreatedDate).Where(x => x.ForiegnEntityId.Equals(Id)).FirstOrDefault();
        //    if (documentEntity != null)
        //    {
        //        return documentEntity.FileURL;
        //    }
        //    else
        //    {
        //        return "/Avatar/Default.jpg";
        //    }
        //}
        //public string GetFileFirstorDefault(Guid? Id, string Title)
        //{
        //    var documentEntity = _context.DocumentEntity.OrderByDescending(x => x.CreatedDate).Where(x => x.ForiegnEntityId.Equals(Id) && x.FileTitle == Title).FirstOrDefault();
        //    if (documentEntity != null)
        //    {
        //        return documentEntity.FileURL;
        //    }
        //    else
        //    {
        //        return "/Avatar/Default.jpg";
        //    }
        //}


        public async Task<String> UploadDocument(FileUploadVM fileUpload)
        {
            var uploadContent = Configuration["StaticFiles:UploadContent"];
            uploadContent = Path.Combine(uploadContent, fileUpload.Path);

            // Ensure the directory exists
            if (!Directory.Exists(uploadContent))
            {
                Directory.CreateDirectory(uploadContent);
            }

            foreach (var formFile in fileUpload.FormFiles)
            {
                if (formFile.Length > 0)
                {
                    var fileExtension = Path.GetExtension(formFile.FileName);
                    var fileNameNew = $"{fileUpload.EntityId}_{DateTime.Now:yyyyMMddHHmmssfff}{fileExtension}";
                    var filePath = Path.Combine(uploadContent, fileNameNew);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await formFile.CopyToAsync(stream);
                    }

                    var documentViewModel = new DocumentEntity()
                    {
                        ForiegnEntityId = fileUpload.EntityId,
                        FileName = fileNameNew,
                        FileType = fileExtension,
                        FileTitle = fileUpload.FileTitle,
                        FileURL = $"/Uploads/{fileUpload.Path}/{fileNameNew}"
                    };

                    await _context.AddAsync(documentViewModel);
                    await _context.SaveChangesAsync();
                }
            }
            return ($"{fileUpload.FormFiles.Count} files uploaded!!");
        }

        public async Task<string> UploadsDocument(FileUploadVM fileUpload)
        {
            var uploadContent = Configuration["StaticFiles:UploadContent"];

            if (!Directory.Exists(uploadContent))
            {
                Directory.CreateDirectory(uploadContent);
            }

            foreach (var formFile in fileUpload.FormFiles)
            {
                if (formFile.Length > 0)
                {
                    var fileExtension = Path.GetExtension(formFile.FileName);
                    var fileNameNew = $"{fileUpload.EntityId}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";
                    var filePath = Path.Combine(uploadContent, fileNameNew);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await formFile.CopyToAsync(stream);
                    }

                    var documentViewModel = new DocumentEntity()
                    {
                        ForiegnEntityId = fileUpload.EntityId,
                        PriorityOrder = fileUpload.PriorityOrder,
                        FileName = fileNameNew,
                        FileTitle = fileUpload.FileTitle,
                        FileType = fileExtension,
                        FileURL = $"/Uploads/{fileUpload.Path}/{fileNameNew}"
                    };
                    if (fileUpload.Id == Guid.Empty || fileUpload.Id == null)
                    {
                        await _context.AddAsync(documentViewModel);
                    }
                    else
                    {
                        documentViewModel.Id = Guid.Parse(fileUpload.Id.ToString());
                        _context.Update(documentViewModel);
                    }
                }
            }

            if (fileUpload.FormFiles.Count == 0)
            {
                //var documentEntity = _context.DocumentEntity.FirstOrDefault(x => x.Id.Equals(fileUpload.Id));
                //documentEntity.FileTitle = fileUpload.FileTitle;
                //documentEntity.PriorityOrder = fileUpload.PriorityOrder;
                //_context.Update(documentEntity);
            }
            await _context.SaveChangesAsync();
            return ($"{fileUpload.FormFiles.Count} files uploaded!!");
        }
    }
}
