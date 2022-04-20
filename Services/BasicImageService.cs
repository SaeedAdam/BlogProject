using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BlogProject.Services
{
    public class BasicImageService : IImageService
    {
        #region CONTENT TYPE
        public string ContentType(IFormFile file)
        {
            return file?.ContentType;
        }
        #endregion

        #region DECODE IMAGE
        public string DecodeImage(byte[] data, string type)
        {
            if (data is null || type is null) return null;

            return $"data:image/{type};base64,{Convert.ToBase64String(data)}";
        }
        #endregion

        #region ENCODE IMAGE
        public async Task<byte[]> EncodeImageAsync(IFormFile file)
        {
            if (file is null) return null;

            using MemoryStream ms = new MemoryStream();

            await file.CopyToAsync(ms);

            return ms.ToArray();

        } 
        #endregion
        
        #region ENCODE IMAGE
        public async Task<byte[]> EncondeImageAsync(string fileName)
        {
            string file = $"{Directory.GetCurrentDirectory()}/wwwroot/images/{fileName}";

            return await File.ReadAllBytesAsync(file);
        }
        #endregion

        #region Size
        public int Size(IFormFile file)
        {
            return Convert.ToInt32(file?.Length);
        } 
        #endregion
    }
}
