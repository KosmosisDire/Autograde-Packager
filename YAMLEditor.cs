using System.Text;

public class YAMLEditor  
{
    string file;

    public YAMLEditor(string file)
    {
        this.file = file;
        var createFile = File.Open(file, FileMode.OpenOrCreate);
        createFile.Dispose();
    }

    public void clearFile()
    {
        FileStream configWrite = File.Open(file, FileMode.Truncate);
        configWrite.Dispose();
    }

    public (int lineNum, string allBefore, string allAfter) FindKey (string key)
    {
        var reader = new StreamReader(file);

        int line_num = 0;
        string line = reader.ReadLine() ?? "";
        
        while (!line.Contains(key+":") && line != "")
        {
            line = reader.ReadLine() ?? "";
            line_num++;
        }

        (string before, string after) = GetSurroundingKeysText(line_num);

        reader.Close();

        return (line_num, before, after);
    }

    public (string before, string after) GetSurroundingKeysText (int thisKeyLineIndex)
    {
        var reader = new StreamReader(file);

        StringBuilder before = new StringBuilder();
        for (int i = 0; i <= thisKeyLineIndex; i++)
            if(i != thisKeyLineIndex)
                before.Append((reader.ReadLine() ?? "") + "\n");
            else
                reader.ReadLine();

        
        string line = reader.ReadLine() ?? "";
        while (!line.Contains(":") && line != "")
        {
            line = reader.ReadLine() ?? "";
        }

        var result = line + reader.ReadToEnd();

        reader.Close();

        return (before.ToString().TrimStart('\n'), result.TrimEnd('\n'));
    }

    public void Update<T>(string key, T value)
    {
        (int lineNum, string allBefore, string allAfter) = FindKey(key);

        clearFile();
        var writer = new StreamWriter(file);

        if(allBefore != "") writer.Write(allBefore);
        writer.Write($"{key}: {value}");
        if(allAfter != "") writer.Write(allAfter);

        writer.Close();
    }

    public void Update<T>(string key, List<T> values)
    {
        (int lineNum, string allBefore, string allAfter) = FindKey(key);

        

        clearFile();
        var writer = new StreamWriter(file);

        if(allBefore != "") writer.Write(allBefore);
        writer.Write($"{key}:\n");
        foreach (var value in values)
            writer.Write($"  - {value}\n");
        if(allAfter != "") writer.Write(allAfter);

        writer.Close();
    }

    public T ReadKey<T>(string key)
    {
        (int lineNum, string allBefore, string allAfter) = FindKey(key);

        var reader = new StreamReader(file);

        for (int i = 0; i < lineNum; i++)
            reader.ReadLine();

        string line = reader.ReadLine() ?? "";

        reader.Close();

        return (T)Convert.ChangeType(line.Split(":")[1].Trim(), typeof(T));
    }

    public List<T> ReadKeyList<T>(string key)
    {
        (int lineNum, string allBefore, string allAfter) = FindKey(key);

        var reader = new StreamReader(file);

        for (int i = 0; i <= lineNum; i++)
            reader.ReadLine();

        string line = reader.ReadLine() ?? "";
        List<T> result = new List<T>();
        while (line.Contains("  - "))
        {
            result.Add((T)Convert.ChangeType(line.Split("-")[1].Trim(), typeof(T)));
            line = reader.ReadLine() ?? "";
        }

        reader.Close();
        
        return result;
    }
}