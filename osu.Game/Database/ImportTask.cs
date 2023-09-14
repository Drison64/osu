// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using osu.Framework.Extensions;
using osu.Game.IO.Archives;
using osu.Game.Utils;
using SharpCompress.Common;

namespace osu.Game.Database
{
    /// <summary>
    /// An encapsulated import task to be imported to an <see cref="RealmArchiveModelImporter{TModel}"/>.
    /// </summary>
    public class ImportTask
    {
        /// <summary>
        /// The path to the file (or filename in the case a stream is provided).
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// An optional stream which provides the file content.
        /// </summary>
        public Stream? Stream { get; }

        /// <summary>
        /// Construct a new import task from a path (on a local filesystem).
        /// </summary>
        public ImportTask(string path)
        {
            Path = path;
        }

        /// <summary>
        /// Construct a new import task from a stream. The provided stream will be disposed after reading.
        /// </summary>
        public ImportTask(Stream stream, string filename)
        {
            Path = filename;
            Stream = stream;
        }

        /// <summary>
        /// Retrieve an archive reader from this task.
        /// </summary>
        public ArchiveReader GetReader()
        {
            if (Stream == null)
                return getReaderFromPath(Path);

            if (Stream is MemoryStream memoryStream)
            {
                if (ZipUtils.IsZipArchive(memoryStream))
                    return new ZipArchiveReader(memoryStream, Path);

                return new MemoryStreamArchiveReader(memoryStream, Path);
            }

            // This isn't used in any current path. May need to reconsider for performance reasons (ie. if we don't expect the incoming stream to be copied out).
            return new ByteArrayArchiveReader(Stream.ReadAllBytesToArray(), Path);
        }

        /// <summary>
        /// Deletes the file that is encapsulated by this <see cref="ImportTask"/>.
        /// </summary>
        public virtual void DeleteFile()
        {
            if (File.Exists(Path))
                File.Delete(Path);
        }

        /// <summary>
        /// Creates an <see cref="ArchiveReader"/> from a valid storage path.
        /// </summary>
        /// <param name="path">A file or folder path resolving the archive content.</param>
        /// <returns>A reader giving access to the archive's content.</returns>
        private ArchiveReader getReaderFromPath(string path)
        {
            if (ZipUtils.IsZipArchive(path))
                return new ZipArchiveReader(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read), System.IO.Path.GetFileName(path));
            if (Directory.Exists(path))
                return new DirectoryArchiveReader(path);
            if (File.Exists(path))
                return new SingleFileArchiveReader(path);

            throw new InvalidFormatException($"{path} is not a valid archive");
        }

        public override string ToString() => System.IO.Path.GetFileName(Path);
    }
}
