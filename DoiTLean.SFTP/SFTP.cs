using System;
using System.IO;
using DoiTLean.SFTP.Structures;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Renci.SshNet;

namespace DoiTLean.SFTP {
    /// <summary>
    ///  The NScrape interface defines the methods for web scrapping.
    /// </summary>
    public class SFTP : ISFTP {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="IP">host (e.g. &quot;127.0.0.1&quot;)</param>
        /// <param name="Port">default: 22</param>
        /// <param name="Username"></param>
        /// <param name="PrivateKey">.pem file content</param>
        /// <param name="Path"></param>
        public void Delete_PrivateKey(string IP, int Port, string Username, byte[] PrivateKey, string Path)
        {
            using (Stream s = new MemoryStream(PrivateKey))
            {
                var keyFile = new PrivateKeyFile(s);
                var keyFiles = new[] { keyFile };
                using (var sftp = new SftpClient(IP, Port, Username, keyFiles))
                {
                    try
                    {
                        sftp.Connect();
                        sftp.DeleteFile(Path);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        sftp.Disconnect();
                    }
                }
            }
        } // Delete_PrivateKey

        /// <summary>
        /// 
        /// </summary>
        /// <param name="IP">host (e.g. &quot;127.0.0.1&quot;)</param>
        /// <param name="Port">default: 22</param>
        /// <param name="Username"></param>
        /// <param name="PrivateKey">.pem file content</param>
        /// <param name="Path"></param>
        /// <param name="Exists"></param>
        public void Exists_PrivateKey(string IP, int Port, string Username, byte[] PrivateKey, string Path, out bool Exists)
        {
            Exists = false;

            using (Stream s = new MemoryStream(PrivateKey))
            {
                var keyFile = new PrivateKeyFile(s);
                var keyFiles = new[] { keyFile };
                using (var sftp = new SftpClient(IP, Port, Username, keyFiles))
                {
                    try
                    {
                        sftp.Connect();
                        Exists = sftp.Exists(Path);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        sftp.Disconnect();
                    }
                }
            }
        } // Exists_PrivateKey

        /// <summary>
        /// 
        /// </summary>
        /// <param name="IP">host (e.g. &quot;127.0.0.1&quot;)</param>
        /// <param name="Port">default: 22</param>
        /// <param name="Username"></param>
        /// <param name="PrivateKey">.pem file content</param>
        /// <param name="Path"></param>
        /// <param name="Data"></param>
        public void Get_PrivateKey(string IP, int Port, string Username, byte[] PrivateKey, string Path, out byte[] Data)
        {
            // Download to a temp file
            Data = new byte[] { };
            string localFile = System.IO.Path.GetTempFileName();

            using (Stream s = new MemoryStream(PrivateKey))
            {
                var keyFile = new PrivateKeyFile(s);
                var keyFiles = new[] { keyFile };
                using (var sftp = new SftpClient(IP, Port, Username, keyFiles))
                {
                    try
                    {
                        sftp.Connect();
                        var file = new FileStream(localFile, FileMode.Create);
                        sftp.DownloadFile(Path, file);
                        file.Close();
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        sftp.Disconnect();
                    }

                    // Read the temp file
                    FileStream fs = new FileStream(localFile, FileMode.Open, FileAccess.Read);
                    if (fs.CanRead)
                    {
                        byte[] buffer = new byte[512];
                        using (MemoryStream ms = new MemoryStream())
                        {
                            while (true)
                            {
                                int read = fs.Read(buffer, 0, buffer.Length);
                                if (read <= 0)
                                {
                                    Data = ms.ToArray();
                                    break;
                                }
                                ms.Write(buffer, 0, read);
                            }
                            fs.Close();
                        }
                    }

                    // Delete temp file
                    File.Delete(localFile);
                }
            }
        } // Get_PrivateKey

        /// <summary>
        /// 
        /// </summary>
        /// <param name="IP">host (e.g. &quot;127.0.0.1&quot;)</param>
        /// <param name="Port">default: 22</param>
        /// <param name="Username"></param>
        /// <param name="PrivateKey">.pem file content</param>
        /// <param name="Path"></param>
        /// <param name="NumberOfFiles">Number of files to fetch. Miing or 0 will return all files.</param>
        /// <param name="List"></param>
        public void List_PrivateKey(string IP, int Port, string Username, byte[] PrivateKey, string Path, int NumberOfFiles, out List<RemoteItem> List)
        {
            List = new List<RemoteItem>();
            RemoteItem rec = new RemoteItem();

            using (Stream s = new MemoryStream(PrivateKey))
            {
                var keyFile = new PrivateKeyFile(s);
                var keyFiles = new[] { keyFile };
                using (var sftp = new SftpClient(IP, Port, Username, keyFiles))
                {
                    try
                    {
                        sftp.Connect();
                        int idx = 0;
                        foreach (var file in sftp.ListDirectory(Path))
                        {
                            if (idx >= NumberOfFiles && NumberOfFiles > 0)
                                break;

                            rec = new RemoteItem();
                            rec.ssFilename = file.Name;
                            rec.ssSizeInBytes = Long2Int(file.Attributes.Size);
                            rec.ssIsDir = file.IsDirectory;
                            rec.ssIsLink = file.IsSymbolicLink;
                            rec.ssCreated = file.LastAccessTime;
                            rec.ssModified = file.LastWriteTime;

                            List.Add(rec);

                            idx++;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        sftp.Disconnect();
                    }
                }
            }
        } // List_PrivateKey

        /// <summary>
        /// 
        /// </summary>
        /// <param name="IP">host (e.g. &quot;127.0.0.1&quot;)</param>
        /// <param name="Port">default: 22</param>
        /// <param name="Username"></param>
        /// <param name="PrivateKey">.pem file content</param>
        /// <param name="Path"></param>
        public void Mkdir_PrivateKey(string IP, int Port, string Username, byte[] PrivateKey, string Path)
        {
            using (Stream s = new MemoryStream(PrivateKey))
            {
                var keyFile = new PrivateKeyFile(s);
                var keyFiles = new[] { keyFile };
                using (var sftp = new SftpClient(IP, Port, Username, keyFiles))
                {
                    try
                    {
                        sftp.Connect();
                        sftp.CreateDirectory(Path);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        sftp.Disconnect();
                    }
                }
            }
        } // Mkdir_PrivateKey

        /// <summary>
        /// 
        /// </summary>
        /// <param name="IP">host (e.g. &quot;127.0.0.1&quot;)</param>
        /// <param name="Port">default: 22</param>
        /// <param name="Username"></param>
        /// <param name="PrivateKey">.pem file content</param>
        /// <param name="Source"></param>
        /// <param name="Target"></param>
        public void Move_PrivateKey(string IP, int Port, string Username, byte[] PrivateKey, string Source, string Target)
        {
            using (Stream s = new MemoryStream(PrivateKey))
            {
                var keyFile = new PrivateKeyFile(s);
                var keyFiles = new[] { keyFile };
                using (var sftp = new SftpClient(IP, Port, Username, keyFiles))
                {
                    try
                    {
                        sftp.Connect();
                        sftp.RenameFile(Source, Target);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        sftp.Disconnect();
                    }
                }
            }
        } // Move_PrivateKey

        /// <summary>
        /// 
        /// </summary>
        /// <param name="IP">host (e.g. &quot;127.0.0.1&quot;)</param>
        /// <param name="Port">default: 22</param>
        /// <param name="Username"></param>
        /// <param name="PrivateKey">.pem file content</param>
        /// <param name="Path"></param>
        /// <param name="Data"></param>
        public void Put_PrivateKey(string IP, int Port, string Username, byte[] PrivateKey, string Path, byte[] Data)
        {
            // Write to a temp file
            string localFile = System.IO.Path.GetTempFileName();
            FileStream fs = new FileStream(localFile, FileMode.Create, FileAccess.Write);
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(Data);
            bw.Close();

            using (Stream s = new MemoryStream(PrivateKey))
            {
                var keyFile = new PrivateKeyFile(s);
                var keyFiles = new[] { keyFile };
                using (var sftp = new SftpClient(IP, Port, Username, keyFiles))
                {
                    try
                    {
                        sftp.Connect();
                        var file = new FileStream(localFile, FileMode.Open);
                        sftp.UploadFile(file, Path);  // Optional: canOverride
                        file.Close();
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        sftp.Disconnect();
                    }

                    // Delete the temp file
                    File.Delete(localFile);
                }
            }
        } // Put_PrivateKey

        /// <summary>
        /// 
        /// </summary>
        /// <param name="IP">host (e.g. &quot;127.0.0.1&quot;)</param>
        /// <param name="Port">default: 22</param>
        /// <param name="Username"></param>
        /// <param name="PrivateKey">.pem file content</param>
        /// <param name="Path"></param>
        public void Rmdir_PrivateKey(string IP, int Port, string Username, byte[] PrivateKey, string Path)
        {
            using (Stream s = new MemoryStream(PrivateKey))
            {
                var keyFile = new PrivateKeyFile(s);
                var keyFiles = new[] { keyFile };
                using (var sftp = new SftpClient(IP, Port, Username, keyFiles))
                {
                    try
                    {
                        sftp.Connect();
                        sftp.DeleteDirectory(Path);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        sftp.Disconnect();
                    }
                }
            }
        } // Rmdir_PrivateKey

        /// <summary>
        /// 
        /// </summary>
        /// <param name="IP">host (e.g. &quot;127.0.0.1&quot;)</param>
        /// <param name="Port">default: 22</param>
        /// <param name="Username"></param>
        /// <param name="PrivateKey">.pem file content</param>
        /// <param name="Path"></param>
        /// <param name="FileName">File name to search</param>
        /// <param name="File"></param>
        public void Search_PrivateKey(string IP, int Port, string Username, byte[] PrivateKey, string Path, string FileName, out RemoteItem File)
        {
            File = new RemoteItem();

            using (Stream s = new MemoryStream(PrivateKey))
            {
                var keyFile = new PrivateKeyFile(s);
                var keyFiles = new[] { keyFile };
                using (var sftp = new SftpClient(IP, Port, Username, keyFiles))
                {
                    try
                    {
                        sftp.Connect();

                        foreach (var file in sftp.ListDirectory(Path))
                        {
                            if (file.Name == FileName)
                            {
                                File.ssFilename = file.Name;
                                File.ssSizeInBytes = Long2Int(file.Attributes.Size);
                                File.ssIsDir = file.IsDirectory;
                                File.ssIsLink = file.IsSymbolicLink;
                                File.ssCreated = file.LastAccessTime;
                                File.ssModified = file.LastWriteTime;
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        sftp.Disconnect();
                    }
                }
            }
        } // Search_PrivateKey


        /// <summary>
        /// 
        /// </summary>
        /// <param name="IP">host (e.g. &quot;127.0.0.1&quot;)</param>
        /// <param name="Port">default: 22</param>
        /// <param name="Username"></param>
        /// <param name="Paword"></param>
        /// <param name="Path"></param>
        /// <param name="Exists"></param>
        public void Exists(string IP, int Port, string Username, string Paword, string Path, out bool Exists)
        {
            Exists = false;

            using (var sftp = new SftpClient(IP, Port, Username, Paword))
            {
                try
                {
                    sftp.Connect();
                    Exists = sftp.Exists(Path);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    sftp.Disconnect();
                }
            }
        } // Exists


        /// <summary>
        /// 
        /// </summary>
        /// <param name="IP"></param>
        /// <param name="Port">host (e.g. &quot;127.0.0.1&quot;)</param>
        /// <param name="Username"></param>
        /// <param name="Paword"></param>
        /// <param name="Path"></param>
        /// <param name="FileName">File name to search</param>
        /// <param name="File"></param>
        public void Search(string IP, int Port, string Username, string Paword, string Path, string FileName, out RemoteItem File)
        {
            File = new RemoteItem();

            using (var sftp = new SftpClient(IP, Port, Username, Paword))
            {
                try
                {
                    sftp.Connect();

                    foreach (var file in sftp.ListDirectory(Path))
                    {
                        if (file.Name == FileName)
                        {
                            File.ssFilename = file.Name;
                            File.ssSizeInBytes = Long2Int(file.Attributes.Size);
                            File.ssIsDir = file.IsDirectory;
                            File.ssIsLink = file.IsSymbolicLink;
                            File.ssCreated = file.LastAccessTime;
                            File.ssModified = file.LastWriteTime;
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    sftp.Disconnect();
                }
            }
        } // Search


        /// <summary>
        /// 
        /// </summary>
        /// <param name="IP">host (e.g. &quot;127.0.0.1&quot;)</param>
        /// <param name="Port">default: 22</param>
        /// <param name="Username"></param>
        /// <param name="Paword"></param>
        /// <param name="Path"></param>
        public void Rmdir(string IP, int Port, string Username, string Paword, string Path)
        {
            using (var sftp = new SftpClient(IP, Port, Username, Paword))
            {
                try
                {
                    sftp.Connect();
                    sftp.DeleteDirectory(Path);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    sftp.Disconnect();
                }
            }
        } // Rmdir


        /// <summary>
        /// 
        /// </summary>
        /// <param name="IP">host (e.g. &quot;127.0.0.1&quot;)</param>
        /// <param name="Port">default: 22</param>
        /// <param name="Username"></param>
        /// <param name="Paword"></param>
        /// <param name="Path"></param>
        public void Mkdir(string IP, int Port, string Username, string Paword, string Path)
        {
            using (var sftp = new SftpClient(IP, Port, Username, Paword))
            {
                try
                {
                    sftp.Connect();
                    sftp.CreateDirectory(Path);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    sftp.Disconnect();
                }
            }
        } // Mkdir

        public void Delete(string IP, int Port, string Username, string Paword, string Path)
        {
            using (var sftp = new SftpClient(IP, Port, Username, Paword))
            {
                try
                {
                    sftp.Connect();
                    sftp.DeleteFile(Path);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    sftp.Disconnect();
                }
            }
        }

        public void Move(string IP, int Port, string Username, string Paword, string Source, string Target)
        {
            using (var sftp = new SftpClient(IP, Port, Username, Paword))
            {
                try
                {
                    sftp.Connect();
                    sftp.RenameFile(Source, Target);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    sftp.Disconnect();
                }
            }
        }

        /// <param name="IP">host (e.g. &quot;127.0.0.1&quot;)</param>
        /// <param name="Port">default: 22</param>
        /// <param name="Username"></param>
        /// <param name="Paword">Plain text paword.</param>
        /// <param name="Path"></param>
        /// <param name="NumberOfFiles">Number of files to fetch. Miing or 0 will return all files.</param>
        /// <param name="List"></param>
        public void List(string IP, int Port, string Username, string Paword, string Path, int NumberOfFiles, out List<RemoteItem> List)
        {
            List = new List<RemoteItem>();
            RemoteItem rec = new RemoteItem();

            using (var sftp = new SftpClient(IP, Port, Username, Paword))
            {
                try
                {
                    sftp.Connect();
                    int idx = 0;
                    foreach (var file in sftp.ListDirectory(Path))
                    {
                        if (idx >= NumberOfFiles && NumberOfFiles > 0)
                            break;

                        rec = new RemoteItem();
                        rec.ssFilename = file.Name;
                        rec.ssSizeInBytes = Long2Int(file.Attributes.Size);
                        rec.ssIsDir = file.IsDirectory;
                        rec.ssIsLink = file.IsSymbolicLink;
                        rec.ssCreated = file.LastAccessTime;
                        rec.ssModified = file.LastWriteTime;

                        List.Add(rec);

                        idx++;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    sftp.Disconnect();
                }
            }
        }

        private int Long2Int(long v)
        {
            if (v >= int.MaxValue) return int.MaxValue;
            else return Convert.ToInt32(v);
        }

        public void Put(string IP, int Port, string Username, string Paword, string Path, byte[] Data)
        {
            // Write to a temp file
            string localFile = System.IO.Path.GetTempFileName();
            FileStream s = new FileStream(localFile, FileMode.Create, FileAccess.Write);
            BinaryWriter bw = new BinaryWriter(s);
            bw.Write(Data);
            bw.Close();

            IP = IP.ToUpper();

            if (IP.Contains("SECURE_GATEWAY") || IP.Contains("SECURE-GATEWAY"))
            {
                IP = IP.Replace("SECURE_GATEWAY", Environment.GetEnvironmentVariable("SECURE_GATEWAY"));
            }        

            // Upload the temp file
            using (var sftp = new SftpClient(IP, Port, Username, Paword))
            {
                try
                {
                    sftp.Connect();
                    var file = new FileStream(localFile, FileMode.Open);
                    sftp.UploadFile(file, Path);  // Optional: canOverride
                    file.Close();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    sftp.Disconnect();
                }
            }

            // Delete the temp file
            File.Delete(localFile);
        }

        public void Get(string IP, int Port, string Username, string Paword, string Path, out byte[] Data)
        {
            // Download to a temp file
            Data = new byte[] { };
            string localFile = System.IO.Path.GetTempFileName();

            using (var sftp = new SftpClient(IP, Port, Username, Paword))
            {
                try
                {
                    sftp.Connect();
                    var file = new FileStream(localFile, FileMode.Create);
                    sftp.DownloadFile(Path, file);
                    file.Close();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    sftp.Disconnect();
                }
            }

            // Read the temp file
            FileStream s = new FileStream(localFile, FileMode.Open, FileAccess.Read);
            if (s.CanRead)
            {
                byte[] buffer = new byte[512];
                using (MemoryStream ms = new MemoryStream())
                {
                    while (true)
                    {
                        int read = s.Read(buffer, 0, buffer.Length);
                        if (read <= 0)
                        {
                            Data = ms.ToArray();
                            break;
                        }
                        ms.Write(buffer, 0, read);
                    }
                    s.Close();
                }
            }

            // Delete temp file
            File.Delete(localFile);
        }


    }
}
