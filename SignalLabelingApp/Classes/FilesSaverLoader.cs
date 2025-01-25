using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using SignalLabelingApp.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SignalLabelingApp.Classes
{
    public static class FileSaverLoader
    {

        /// <summary>
        /// Выбор файла и создание редактора
        /// </summary>
        /// <returns></returns>
        public static EditorBase LoadEditorFromFile()
        {

            string fileToReadPath = Task.Run(async () => await GetFilePathAsync()).GetAwaiter().GetResult();

            EditorBase Editor = CreateNewEditor(fileToReadPath);

            return Editor;
        }

        /// <summary>
        /// Создание редактора по файлу
        /// </summary>
        /// <param name="fileToReadPath"></param>
        /// <returns></returns>
        public static EditorBase CreateNewEditor(string fileToReadPath)
        {
            string fileExtension = Path.GetExtension(fileToReadPath);
            if (fileExtension is not null)
            {
                EditorBase Editor = Globals.FileExtentionToEditor[fileExtension];
                Editor.LoadFromFile(fileToReadPath);
                return Editor;
            }
            return null;

        }

        /// <summary>
        /// Выбор файла
        /// </summary>
        /// <returns></returns>
        public static async Task<string> GetFilePathAsync()
        {
            string filePath = null;

            // Получаем TopLevel из текущего окна
            var topLevel = TopLevel.GetTopLevel(Globals.MainEditorControl);

            if (topLevel != null)
            {
                var storageProvider = topLevel.StorageProvider;

                if (storageProvider == null)
                {
                    return null; // StorageProvider недоступен
                }

                // Формируем фильтры для файлов
                var fileTypesFilter = new List<FilePickerFileType>();

                foreach (var fileType in Globals.FileExtentionToEditor)
                {
                    fileTypesFilter.Add(new FilePickerFileType($"Файлы {fileType.Key}")
                    {
                        Patterns = new[] { $"*{fileType.Key}" }
                    });
                }

                fileTypesFilter.Add(new FilePickerFileType("Все файлы")
                {
                    Patterns = new[] { "*.*" }
                });

                // Открываем диалог выбора файла
                var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    AllowMultiple = false,
                    FileTypeFilter = fileTypesFilter,
                });

                if (files != null && files.Count > 0)
                {
                    var selectedFile = files[0];
                    filePath = selectedFile.Path.AbsolutePath;

                    if (filePath == null)
                    {
                        filePath = selectedFile.Path.AbsoluteUri;
                    }
                }
            }

            return filePath;
        }

        /// <summary>
        /// Выбор папки
        /// </summary>
        /// <returns></returns>
        public static async Task<string> GetSaveDirPathAsync()
        {
            string saveDirPath = null;

            // Получаем TopLevel из текущего окна
            var topLevel = TopLevel.GetTopLevel(Globals.MainEditorControl);

            if (topLevel != null)
            {
                var storageProvider = topLevel.StorageProvider;

                if (storageProvider == null)
                {
                    return null;
                }

                var folder = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                {
                    AllowMultiple = false,
                    Title = "Выберите директорию для сохранения"
                });

                if (folder != null && folder.Count > 0)
                {
                    var selectedFolder = folder[0];
                    saveDirPath = selectedFolder.Path.AbsolutePath;

                    if (saveDirPath == null)
                    {
                        saveDirPath = selectedFolder.Path.AbsoluteUri;
                    }
                }
            }

            return saveDirPath;
        }

        /// <summary>
        /// Сохранение произвольного объекта в файл
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objToSave"></param>
        /// <param name="fileName"></param>
        /// <param name="dirPath"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static void SaveToJson<T>(T objToSave, string fileName, string dirPath)
        {
            if (objToSave == null)
            {
                throw new ArgumentNullException(nameof(objToSave), "Object to save cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));
            }

            if (string.IsNullOrWhiteSpace(dirPath))
            {
                throw new ArgumentException("Directory path cannot be null or empty.", nameof(dirPath));
            }

            try
            {
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                string filePath = Path.Combine(dirPath, fileName);

                var jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                };

                string jsonString = JsonSerializer.Serialize(objToSave, jsonOptions);

                File.WriteAllText(filePath, jsonString);

                Console.WriteLine($"Object saved successfully to {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while saving the object: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Генерация случайного не существующего в папке названия файла
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="postfix"></param>
        /// <param name="dirPath"></param>
        /// <returns></returns>
        public static string GenerateUniqueFileName(string prefix, string postfix, string dirPath)
        {
            Random random = new Random();
            string resName = "";
            string filePath = "";
            do
            {
                resName = $"{prefix}_{random.NextDouble().ToString().Substring(2)}{postfix}";
                filePath = $"{dirPath}{resName}";
            }
            while (File.Exists(filePath));

            return resName;
            
        }
    }
}


