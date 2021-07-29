#nullable enable
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FileInfoExtensions;

// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global

// ReSharper disable UnusedMember.Global

// ReSharper disable once CheckNamespace
namespace System.IO
{
    /// <summary>Класс методов расширений для объектов класса System.IO.FileInfo</summary>
    public static class FileInfoExtensions
    {
        #region Names
        private static readonly HashSet<char> __NotValidPathChars = new(Path.GetInvalidFileNameChars());

        /// <summary>Подбор свободного имени для шаблона отчетов</summary>
        /// <param name="Names">Список занятых имен</param>
        /// <param name="DefaultName">Имя по умолчанию</param>
        /// <param name="NamePattern">паттерн составления имени</param>
        /// <returns></returns>
        public static string GetFreeName(this string DefaultName, IEnumerable<string> Names, string NamePattern = "{0} ({1})")
        {
            var name = DefaultName;
            var names = new HashSet<string>(Names);
            for (var i = 1; names.Contains(name); i++)
                name = string.Format(NamePattern, DefaultName, i);
            return name;
        }

        /// <summary>Возвращает откорректированное имя файла с точки зрения файловой системы</summary>
        /// <param name="Name">Текущее имя</param>
        /// <param name="ReplacementSymbol">Символ на который будут заменены запрещенные</param>
        /// <returns>Корректное имя файла с точки зрения файловой системы</returns>
        public static string GetValidFileName(this string Name, char ReplacementSymbol = '_')
        {
            if (string.IsNullOrWhiteSpace(Name))
                return "new_file";

            var new_name = Name.Trim(' ');

            return __NotValidPathChars.Aggregate(new_name, (current, c) => current.Replace(c, ReplacementSymbol));
        }

        /// <summary>
        /// Проверяет имя файла на валидность
        /// </summary>
        /// <param name="Name">имя файла</param>
        /// <returns>истина или лож</returns>
        public static bool CheckValidFileName(this string Name) =>
            !string.IsNullOrWhiteSpace(Name) && !Name.Trim(' ').Any(__NotValidPathChars.Contains);


        #endregion

        #region Binary

        /// <summary>Создать двоичный файл</summary>
        /// <param name="File">Информация о создаваемом файле</param>
        /// <returns>Объект для записи данных в двоичный файл</returns>
        public static BinaryWriter CreateBinary(this FileInfo File) => new(File.Create());

        /// <summary>Создать двоичный файл</summary>
        /// <param name="File">Информация о создаваемом файле</param>
        /// <param name="BufferLength">Размер буфера записи</param>
        /// <returns>Объект для записи данных в двоичный файл</returns>
        public static BinaryWriter CreateBinary(this FileInfo File, int BufferLength) =>
            new(new FileStream(File.FullName, FileMode.Create, FileAccess.Write, FileShare.None, BufferLength));

        /// <summary>Создать двоичный файл</summary>
        /// <param name="File">Информация о создаваемом файле</param>
        /// <param name="Encoding">Кодировка</param>
        /// <returns>Объект для записи данных в двоичный файл</returns>
        public static BinaryWriter CreateBinary(this FileInfo File, Encoding Encoding) =>
            new(File.Create(), Encoding);

        /// <summary>Создать двоичный файл</summary>
        /// <param name="File">Информация о создаваемом файле</param>
        /// <param name="BufferLength">Размер буфера записи</param>
        /// <param name="Encoding">Кодировка</param>
        /// <returns>Объект для записи данных в двоичный файл</returns>
        public static BinaryWriter CreateBinary(this FileInfo File, int BufferLength, Encoding Encoding) =>
            new(new FileStream(File.FullName, FileMode.Create, FileAccess.Write, FileShare.None, BufferLength), Encoding);

        /// <summary>Открыть двоичный файл для чтения</summary>
        /// <param name="File">Информация о создаваемом файле</param>
        /// <returns>Объект для чтения данных из двоичного файла</returns>
        public static BinaryReader OpenBinary(this FileInfo File) => new(File.OpenRead());

        /// <summary>Открыть двоичный файл для чтения</summary>
        /// <param name="File">Информация о создаваемом файле</param>
        /// <param name="BufferLength">Размер буфера чтения</param>
        /// <returns>Объект для чтения данных из двоичного файла</returns>
        public static BinaryReader OpenBinary(this FileInfo File, int BufferLength) =>
            new(new FileStream(File.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, BufferLength));

        /// <summary>Открыть двоичный файл для чтения</summary>
        /// <param name="File">Информация о создаваемом файле</param>
        /// <param name="Encoding">Кодировка</param>
        /// <returns>Объект для чтения данных из двоичного файла</returns>
        public static BinaryReader OpenBinary(this FileInfo File, Encoding Encoding) =>
            new(File.OpenRead(), Encoding);

        /// <summary>Открыть двоичный файл для чтения</summary>
        /// <param name="File">Информация о создаваемом файле</param>
        /// <param name="BufferLength">Размер буфера чтения</param>
        /// <param name="Encoding">Кодировка</param>
        /// <returns>Объект для чтения данных из двоичного файла</returns>
        public static BinaryReader OpenBinary(this FileInfo File, int BufferLength, Encoding Encoding) =>
            new(new FileStream(File.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, BufferLength), Encoding);

        #endregion

        #region READ

        /// <summary>Признак конца потока</summary>
        /// <param name="reader">Объект чтения потока</param>
        /// <returns>Истина, если поток закончен</returns>
        public static bool IsEOF(this BinaryReader reader) => reader.BaseStream.Position == reader.BaseStream.Length;
        public static IEnumerable<string?> GetStringLines(this FileInfo File)
        {
            using var reader = File.OpenText();
            while (!reader.EndOfStream)
                yield return reader.ReadLine();
        }

        public static IEnumerable<string?> GetStringLines(this FileInfo File, Encoding encoding)
        {
            using var reader = new StreamReader(File.FullName, encoding);
            while (!reader.EndOfStream)
                yield return reader.ReadLine();
        }

        public static IEnumerable<byte> ReadBytes(this FileInfo file)
        {
            using var stream = file.ThrowIfNotFound().OpenRead();
            using var reader = new BinaryReader(stream);
            while (!reader.IsEOF())
                yield return reader.ReadByte();
        }

        public static IEnumerable<byte[]>? ReadBytes(this FileInfo file, int Length)
        {
            using var stream = file.ThrowIfNotFound().OpenRead();
            using var reader = new BinaryReader(stream);
            while (!reader.IsEOF())
                yield return reader.ReadBytes(Length);
        }

        public static IEnumerable<string?> ReadLines(this FileInfo file)
        {
            using var reader = file.ThrowIfNotFound().OpenText();
            while (!reader.EndOfStream)
                yield return reader.ReadLine();
        }

        public static IEnumerable<string?> ReadLines(
            this FileInfo file,
            Action<StreamReader>? initializer,
            int BufferSize = 3 * DataLength.Bytes.MB)
        {
            using var reader = new StreamReader(
                new BufferedStream(file.ThrowIfNotFound().Open(
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.Read),
                    BufferSize));
            initializer?.Invoke(reader);
            while (!reader.EndOfStream)
                yield return reader.ReadLine();
        }

        public static string? ReadAllText(this FileInfo file, bool ThrowNotExist = true)
        {
            if (!file.Exists && !ThrowNotExist) return null;
            using var reader = file.OpenText();
            return reader.ReadToEnd();
        }

        #endregion

        #region WRITE

        /// <summary>Записать массив байт в файл</summary>
        /// <param name="file">Файл данных</param>
        /// <param name="Data">Данные</param>
        /// <param name="Append">Если истина, то данные будут добавлены в конец файла</param>
        public static void WriteAllBytes(this FileInfo file, byte[] Data, bool Append = false)
        {
            using var stream = new FileStream(file.FullName, Append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.Read);
            stream.Write(Data, 0, Data.Length);
        }

        public static void Append(this FileInfo file, string Text)
        {
            using var writer = new StreamWriter(file.Open(FileMode.Append, FileAccess.Write, FileShare.Read));
            writer.Write(Text);
        }

        public static void AppendLine(this FileInfo file, string Text)
        {
            using var writer = new StreamWriter(file.Open(FileMode.Append, FileAccess.Write, FileShare.Read));
            writer.WriteLine(Text);
        }

        public static void Append(this FileInfo file, byte[] buffer)
        {
            using var writer = new StreamWriter(file.Open(FileMode.Append, FileAccess.Write, FileShare.Read));
            writer.Write(buffer);
        }

        public static FileStream Append(this FileInfo File) => File.Open(FileMode.Append, FileAccess.Write);

        #endregion

        #region MOVE

        public static FileInfo MoveTo(this FileInfo SourceFile, FileInfo DestinationFile, bool Override = true)
        {
            DestinationFile.Refresh();
            if (DestinationFile.Exists && !Override) return DestinationFile;
            SourceFile.MoveTo(DestinationFile.FullName);
            DestinationFile.Refresh();
            SourceFile.Refresh();
            return DestinationFile;
        }

        public static FileInfo MoveTo(this FileInfo SourceFile, DirectoryInfo Destination, bool Override = false)
        {
            var destination_file = Destination.CreateFileInfo(SourceFile.Name);
            if (destination_file.Exists)
                if (Override)
                    destination_file.Delete();
                else
                    return destination_file;

            SourceFile.MoveTo(destination_file);
            destination_file.Refresh();
            SourceFile.Refresh();
            return destination_file;
        }


        #endregion

        #region COPY

        /// <summary>Скопировать файл в директорию</summary>
        /// <param name="SourceFile">Файл источник</param>
        /// <param name="DestinationDirectory">Директория назначения</param>
        /// <returns>Файл копия</returns>
        public static FileInfo CopyTo(this FileInfo SourceFile, DirectoryInfo DestinationDirectory) =>
            SourceFile.CopyTo(Path.Combine(DestinationDirectory.FullName, Path.GetFileName(SourceFile.Name)));

        /// <summary>Скопировать файл в директорию</summary>
        /// <param name="SourceFile">Файл источник</param>
        /// <param name="DestinationDirectory">Директория назначения</param>
        /// <param name="Overwrite">Перезаписать в случае наличия файла</param>
        /// <returns>Файл копия</returns>
        public static FileInfo CopyTo(this FileInfo SourceFile, DirectoryInfo DestinationDirectory, bool Overwrite)
        {
            var new_file = Path.Combine(DestinationDirectory.FullName, Path.GetFileName(SourceFile.Name));
            return !Overwrite && File.Exists(new_file) ? new FileInfo(new_file) : SourceFile.CopyTo(new_file, true);
        }

        /// <summary>Скопировать файл</summary>
        /// <param name="SourceFile">Файл источник</param>
        /// <param name="DestinationFile">Файл копия</param>
        public static void CopyTo(this FileInfo SourceFile, FileInfo DestinationFile) =>
            SourceFile.CopyTo(DestinationFile.FullName);

        /// <summary>Скопировать файл</summary>
        /// <param name="SourceFile">Файл источник</param>
        /// <param name="DestinationFile">Файл копия</param>
        /// <param name="Overwrite">Перезаписать в случае наличия файла</param>
        public static void CopyTo(this FileInfo SourceFile, FileInfo DestinationFile, bool Overwrite) =>
            SourceFile.CopyTo(DestinationFile.FullName, Overwrite);


        #endregion

        #region Execute process

        /// <summary>Выполнить файл с правами администратора</summary>
        /// <param name="File">Исполняемый файл</param>
        /// <param name="Args">Аргументы командной строки</param>
        /// <param name="UseShellExecute">Использовать интерпретацию файла операционной системой</param>
        /// <returns>Созданный процесс</returns>
        public static Process? ExecuteAsAdmin(this FileInfo File, string Args = "", bool UseShellExecute = true) =>
            // ReSharper disable once StringLiteralTypo
            File.Execute(Args, UseShellExecute, "runas");

        /// <summary>Выполнить файл</summary>
        /// <param name="File">Исполняемый файл</param>
        /// <param name="Args">Аргументы командной строки</param>
        /// <param name="UseShellExecute">Использовать интерпретацию файла операционной системой</param>
        /// <param name="Verb"></param>
        /// <returns>Созданный процесс</returns>
        public static Process? Execute(this FileInfo File, string Args, bool UseShellExecute, string Verb) =>
            Process.Start(new ProcessStartInfo(File.FullName, Args)
            {
                UseShellExecute = UseShellExecute,
                Verb = Verb
            });

        public static Process Execute(string File, string Args = "", bool UseShellExecute = true) => Process.Start(new ProcessStartInfo(File, Args) { UseShellExecute = UseShellExecute }).NotNull();

        public static Process Execute(this FileInfo File, string Args = "", bool UseShellExecute = true) => Process.Start(new ProcessStartInfo(UseShellExecute ? File.ToString() : File.FullName, Args) { UseShellExecute = UseShellExecute }).NotNull();


        public static Process? ShowInExplorer(this FileSystemInfo File) => Process.Start("explorer", $"/select,\"{File.FullName}\"");

        #endregion

        #region ZIP

        public static FileInfo Zip(this FileInfo File, string? ArchiveFileName = null, bool Override = true)
        {
            if (File is null) throw new ArgumentNullException(nameof(File));
            File.ThrowIfNotFound();

            if (ArchiveFileName is null) ArchiveFileName = $"{File.FullName}.zip";
            else if (!Path.IsPathRooted(ArchiveFileName))
                ArchiveFileName = (File.Directory ?? throw new InvalidOperationException($"Не удалось получить директорию файла {File}")).CreateFileInfo(ArchiveFileName).FullName;
            using var zip_stream = IO.File.Open(ArchiveFileName, FileMode.OpenOrCreate, FileAccess.Write);
            using var zip = new ZipArchive(zip_stream);
            var file_entry = zip.GetEntry(File.Name);
            if (file_entry != null)
            {
                if (!Override) return new FileInfo(ArchiveFileName);
                file_entry.Delete();
            }

            using var file_entry_stream = zip.CreateEntry(File.Name).Open();
            using var file_stream = File.OpenRead();
            file_stream.CopyTo(file_entry_stream);

            return new FileInfo(ArchiveFileName);
        }

        public static async Task<FileInfo> ZipAsync(
            this FileInfo File,
            byte[] Buffer,
            string? ArchiveFileName = null,
            bool Override = true,
            IProgress<double>? Progress = null,
            CancellationToken Cancel = default)
        {
            if (File is null) throw new ArgumentNullException(nameof(File));
            File.ThrowIfNotFound();
            Cancel.ThrowIfCancellationRequested();

            if (ArchiveFileName is null)
                ArchiveFileName = $"{File.FullName}.zip";
            else if (!Path.IsPathRooted(ArchiveFileName))
                ArchiveFileName = (File.Directory ?? throw new InvalidOperationException($"Не удалось получить директорию файла {File}")).CreateFileInfo(ArchiveFileName).FullName;
            using var zip_stream = IO.File.Open(ArchiveFileName, FileMode.OpenOrCreate, FileAccess.Write);
            using var zip = new ZipArchive(zip_stream);
            var file_entry = zip.GetEntry(File.Name);
            if (file_entry != null)
            {
                if (!Override) return new FileInfo(ArchiveFileName);
                file_entry.Delete();
            }

            using var file_entry_stream = zip.CreateEntry(File.Name).Open();
            using var file_stream = File.OpenRead();
            if (Progress is null)
                await file_stream.CopyToAsync(file_entry_stream, Buffer, Cancel).ConfigureAwait(false);
            else
                await file_stream.CopyToAsync(file_entry_stream, Buffer, file_stream.Length, Progress, Cancel).ConfigureAwait(false);

            return new FileInfo(ArchiveFileName);
        }

        #endregion

        /// <summary>Проверка на существование файла. Если файл не существует, то генерируется исключение</summary>
        /// <param name="file">Проверяемый файл</param>
        /// <param name="Message">Сообщение, добавляемое в исключение</param>
        /// <returns>Информация о файле</returns>
        /// <exception cref="FileNotFoundException">если файл не существует</exception>
        public static FileInfo ThrowIfNotFound(this FileInfo file, string? Message = null)
        {
            file.Refresh();
            return file.Exists ? file : throw new FileNotFoundException(Message ?? $"Файл {file} не найден");
        }

        /// <summary>Вычислить хеш-сумму SHA256</summary>
        /// <param name="file">Файл, контрольную сумму которого надо вычислить</param>
        /// <returns>Массив байт контрольной суммы</returns>
        public static byte[] ComputeSHA256(this FileInfo file)
        {
            if (file is null) throw new ArgumentNullException(nameof(file));
            using var stream = file.ThrowIfNotFound().OpenRead();
            using var md5 = new Security.Cryptography.SHA256Managed();
            return md5.ComputeHash(stream);
        }

        public static byte[] ComputeMD5(this FileInfo file)
        {
            if (file is null) throw new ArgumentNullException(nameof(file));
            using var stream = file.ThrowIfNotFound().OpenRead();
            using var md5 = new Security.Cryptography.MD5CryptoServiceProvider();
            return md5.ComputeHash(stream);
        }

        /// <summary>Получить имя файла без расширения</summary>
        /// <param name="file">Файл</param>
        /// <returns>Имя файла без расширения</returns>
        public static string GetFileNameWithoutExtension(this FileInfo file) => Path.GetFileNameWithoutExtension(file.Name);

        /// <summary>Получить имя файла без расширения</summary>
        /// <param name="file">Файл</param>
        /// <returns>Имя файла без расширения</returns>
        public static string GetFullFileNameWithoutExtension(this FileInfo file) =>
            Path.Combine(file.Directory!.FullName, file.GetFileNameWithoutExtension());

        /// <summary>Получить имя файла c новым расширением</summary>
        /// <param name="file">Файл</param>
        /// <param name="NewExt">Новое расширение файла в формате ".exe"</param>
        /// <returns>Имя файла без расширения</returns>
        public static string GetFullFileNameWithNewExtension(this FileInfo file, string NewExt) =>
            Path.ChangeExtension(file.FullName, NewExt);
        public static FileInfo ChangeExtension(this FileInfo File, string? NewExtension) => new(Path.ChangeExtension(File.ParamNotNull(nameof(File)).FullName, NewExtension));
        public static async Task CheckFileAccessAsync(this FileInfo File, int Timeout = 1000, int IterationCount = 100, CancellationToken Cancel = default)
        {
            if (File is null) throw new ArgumentNullException(nameof(File));
            File.ThrowIfNotFound();
            for (var i = 0; i < IterationCount; i++)
                try
                {
                    Cancel.ThrowIfCancellationRequested();
                    using var stream = File.Open(FileMode.Open, FileAccess.Read);
                    if (stream.Length > 0) return;
                }
                catch (IOException)
                {
                    await Task.Delay(Timeout, Cancel).ConfigureAwait(false);
                }
            Cancel.ThrowIfCancellationRequested();
            throw new InvalidOperationException($"Файл {File.FullName} заблокирован другим процессом");
        }


        #region Sub class extensions

        internal static Task CopyToAsync(this Stream input, Stream output, int BufferLength = 0x1000, CancellationToken Cancel = default) =>
            BufferLength < 1
                ? throw new ArgumentOutOfRangeException(nameof(BufferLength), "Длина буфера копирования менее одного байта")
                : input.CopyToAsync(output, new byte[BufferLength], Cancel);
        internal static async Task CopyToAsync(
            this Stream input,
            Stream output,
            byte[] Buffer,
            CancellationToken Cancel = default)
        {
            if (input is null) throw new ArgumentNullException(nameof(input));
            if (!input.CanRead) throw new ArgumentException("Входной поток недоступен для чтения", nameof(input));
            if (output is null) throw new ArgumentNullException(nameof(output));
            if (!output.CanWrite) throw new ArgumentException("Выходной поток недоступен для записи", nameof(output));

            if (Buffer is null) throw new ArgumentNullException(nameof(Buffer));
            if (Buffer.Length == 0) throw new ArgumentException("Размер буфера для копирования равен 0", nameof(Buffer));

            var buffer_length = Buffer.Length;
            int readed;
            do
            {
                Cancel.ThrowIfCancellationRequested();
                readed = await input.ReadAsync(Buffer, 0, buffer_length, Cancel).ConfigureAwait(false);
                if (readed == 0) continue;
                Cancel.ThrowIfCancellationRequested();
                await output.WriteAsync(Buffer, 0, readed, Cancel).ConfigureAwait(false);
            } while (readed > 0);
        }

        internal static async Task CopyToAsync(
            this Stream input,
            Stream output,
            byte[] Buffer,
            long Length,
            IProgress<double>? Progress = null,
            CancellationToken Cancel = default)
        {
            if (input is null) throw new ArgumentNullException(nameof(input));
            if (!input.CanRead) throw new ArgumentException("Входной поток недоступен для чтения", nameof(input));
            if (output is null) throw new ArgumentNullException(nameof(output));
            if (!output.CanWrite) throw new ArgumentException("Выходной поток недоступен для записи", nameof(output));

            if (Buffer is null) throw new ArgumentNullException(nameof(Buffer));
            if (Buffer.Length == 0) throw new ArgumentException("Размер буфера для копирования равен 0", nameof(Buffer));

            var buffer_length = Buffer.Length;
            int readed;
            var total_readed = 0;
            var last_percent = 0d;
            do
            {
                Cancel.ThrowIfCancellationRequested();
                readed = await input
                   .ReadAsync(Buffer, 0, (int)Math.Min(buffer_length, Length - total_readed), Cancel)
                   .ConfigureAwait(false);
                if (readed == 0) continue;
                total_readed += readed;
                Cancel.ThrowIfCancellationRequested();
                await output.WriteAsync(Buffer, 0, readed, Cancel).ConfigureAwait(false);
                var percent = (double)total_readed / Length;
                if (percent - last_percent >= 0.01)
                    Progress?.Report(last_percent = percent);
            } while (readed > 0 && total_readed < Length);
        }
        /// <summary>Проверка параметра на <see langword="null"/></summary>
        /// <typeparam name="T">Тип параметра <paramref name="obj"/></typeparam>
        /// <param name="obj">Проверяемый на <see langword="null"/> объект</param>
        /// <param name="ParameterName">Имя параметра для указания его в исключении</param>
        /// <param name="Message">Сообщение ошибки</param>
        /// <returns>Объект, гарантированно не <see langword="null"/></returns>
        /// <exception cref="T:System.ArgumentException">Если параметр <paramref name="obj"/> == <see langword="null"/>.</exception>

        internal static T ParamNotNull<T>(this T? obj, string ParameterName, string? Message = null) where T : class =>
            obj ?? throw new ArgumentException(Message ?? $"Отсутствует ссылка для параметра {ParameterName}", ParameterName);
        /// <summary>Проверка на пустую ссылку</summary>
        /// <typeparam name="T">Тип проверяемого объекта</typeparam>
        /// <param name="obj">Проверяемое значение</param>
        /// <param name="Message">Сообщение ошибки</param>
        /// <returns>Значение, точно не являющееся пустой ссылкой</returns>
        /// <exception cref="InvalidOperationException">В случае если переданное значение <paramref name="obj"/> == null</exception>

        internal static T NotNull<T>(this T? obj, string? Message = null) where T : class => obj ?? throw new InvalidOperationException(Message ?? "Пустая ссылка на объект");
        /// <summary>Создать объект с информацией о вложенном файле</summary>
        /// <param name="directory">Родительская директория</param>
        /// <param name="FileRelativePath">Относительный путь к файлу внутри директории</param>
        /// <returns>Фал по указанному пути внутри директории</returns>
        internal static FileInfo CreateFileInfo(this DirectoryInfo directory, string FileRelativePath) =>
            new(Path.Combine(directory.FullName, FileRelativePath.Replace(':', '.')));

        #endregion
    }
}