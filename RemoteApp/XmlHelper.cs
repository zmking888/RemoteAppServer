using System.Text;
using System.IO;
using System.Collections.Generic;
using System;
using System.Xml.Serialization;
using System.Xml;
using RemoteApp;
public static class XmlHelper
{
    public static string XmlSerialize(object o, Encoding encoding)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            XmlSerializeInternal(stream, o, encoding);

            stream.Position = 0;
            using (StreamReader reader = new StreamReader(stream, encoding))
            {
                return reader.ReadToEnd();
            }
        }
    }

    ///
    /// 从XML字符串中反序列化对象
    ///
    /// 结果对象类型
    /// 包含对象的XML字符串 /// 编码方式 /// 反序列化得到的对象
    public static Object XmlDeserialize(string s, Encoding encoding)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            using (StreamWriter sw = new StreamWriter(stream, Encoding.UTF8))
            {
                sw.Write(s);
                sw.Flush();
                stream.Seek(0, SeekOrigin.Begin);
                XmlSerializer serializer = new XmlSerializer(typeof(Configs));
                try
                {
                    return (serializer.Deserialize(stream));
                }
                catch { return default(object); }
            }
        }

    }

    private static void XmlSerializeInternal(Stream stream, object o, Encoding encoding)
    {
        if (o == null)
            throw new ArgumentNullException("o");
        if (encoding == null)
            throw new ArgumentNullException("encoding");

        XmlSerializer serializer = new XmlSerializer(o.GetType());

        XmlWriterSettings settings = new XmlWriterSettings();
        settings.Indent = true;
        settings.NewLineChars = "\r\n";
        settings.Encoding = encoding;
        settings.IndentChars = " ";

        using (XmlWriter writer = XmlWriter.Create(stream, settings))
        {
            serializer.Serialize(writer, o);
            writer.Close();
        }
    }

    ///
    /// 将一个对象按XML序列化的方式写入到一个文件
    ///
    /// 要序列化的对象 /// 保存文件路径 /// 编码方式 
    public static void XmlSerializeToFile(object o, string path, Encoding encoding)
    {
        if (o == null) throw new ArgumentNullException("o");
        XmlSerializer serializer = new XmlSerializer(typeof(Configs));
        MemoryStream stream = new MemoryStream();
        XmlTextWriter xtw = new XmlTextWriter(stream, encoding);
        xtw.Formatting = Formatting.Indented;
        try
        {
            serializer.Serialize(stream, o);
        }
        catch { throw new ArgumentNullException("o"); }
        stream.Position = 0;
        string returnStr = string.Empty;
        using (StreamReader sr = new StreamReader(stream,encoding))
        {
            string line = "";
            while ((line = sr.ReadLine()) != null)
            {
                returnStr += line;
            }
        } 
        
        using (FileStream file = new FileStream(path, FileMode.Create, FileAccess.Write))
        {
            XmlSerializeInternal(file, o, encoding);
        }
    }

    ///
    /// 读入一个文件，并按XML的方式反序列化对象。
    ///
    /// 结果对象类型
    /// 文件路径 /// 编码方式 /// 反序列化得到的对象
    public static Object XmlDeserializeFromFile(string path, Encoding encoding)
    {
        if (string.IsNullOrEmpty(path))
            throw new ArgumentNullException("path");
        if (encoding == null)
            throw new ArgumentNullException("encoding");
        string xml = File.ReadAllText(path, encoding);
        return XmlDeserialize(xml, encoding);
    }

}