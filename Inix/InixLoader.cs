using System.Text;
using System.Xml.Linq;

namespace Inix
{
    public class InixProperty
    {
        public InixProperty(string value, string comment = "")
        {
            this.value = value;
            this.comment = comment;
        }

        public string value { get; set; } = "";
        public string comment { get; set; } = "";
    }

    public class InixFile
    {
        public Dictionary<string, InixObject> inixObjects = new();

        public List<string> errors = new();

        public bool hasErrors
        {
            get => errors.Count > 0;
        }

        public InixObject this[string key]
        {
            get => inixObjects[cleanseKey(key)];
            set => inixObjects[cleanseKey(key)] = value;
        }

        public InixObject getComment(int commentNumber)
        {
            return inixObjects[$"Comment-{commentNumber}"];
        }

        public override string ToString()
        {
            if (inixObjects.Count == 0)
            {
                InixLogger.log("Trying to convert to string but ini data is empty. (Hint: run inix.parse())");

                return "<empty>";
            }

            return reconstruct();
        }

        private string reconstruct()
        {
            StringBuilder returnObject = new();

            foreach (KeyValuePair<string, InixObject> objects in inixObjects)
            {
                switch (objects.Value.type)
                {
                    case InixType.Header:

                        //Ensure that we include any comments.
                        string headerComment = (string.IsNullOrEmpty(objects.Value.comment)) ? "" : " ; " + objects.Value.comment;

                        //Append the header title
                        returnObject.AppendLine($"{objects.Key}{headerComment}");

                        //Add all of the properties under the header.
                        foreach (KeyValuePair<string, InixProperty> properties in objects.Value.properties)
                        {
                            //Get the entire property string including comments.
                            string property = $"{properties.Key}={properties.Value.value}" + (string.IsNullOrEmpty(properties.Value.comment) ? "" : " ; " + properties.Value.comment);

                            returnObject.AppendLine(property);
                        }

                        //Add a new line
                        returnObject.AppendLine("");

                        break;
                    case InixType.Comment:

                        //Append the comments.
                        returnObject.AppendLine(objects.Value.comment);
                        returnObject.AppendLine("");

                        break;
                }
            }

            return returnObject.ToString();
        }

        private string cleanseKey(string key)
        {
            //Check if it contains a bracket.
            bool containsBracket = (key.Trim()[0] == '[');

            //If it doesn't contain a bracket, add it in, user does not know this is occuring.
            return (containsBracket) ? key : string.Format("[{0}]", key);
        }

        public int objectCount()
        {
            return inixObjects.Count();
        }

        public bool containsHeader(string key)
        {
            return inixObjects.ContainsKey(cleanseKey(key));
        }

        public void printDictionary()
        {
            foreach (KeyValuePair<string, InixObject> keyVal in inixObjects)
            {
                InixLogger.log($"{keyVal.Key} -> {keyVal.Value.type}");
            }
        }
    }

    public class InixObject
    {
        public InixObject(InixType type)
        {
            this.type = type;

            properties = new();
        }

        public InixObject(InixType type, string comment)
        {
            this.type = type;
            this.comment = comment;
        }

        public InixType type { get; set; } = InixType.Comment;

        public Dictionary<string, InixProperty>? properties { get; set; } = null;

        public string comment { get; set; } = "";

        public InixProperty this[string key]
        {
            get => properties[key];
            set => properties[key] = value;
        }
    }

    public enum InixType
    {
        Property,
        Header,
        Comment
    }

    public class InixLoader
    {
        private string[]? contents = null;
        private int commentCount = 0;
        private string lastHeader = "";
        private InixFile inixFile = new();

        public InixFile parse(string path)
        {
            //Cleanup to ensure everything is empty.
            cleanup();

            //Load the file into the contents array.
            loadFile(path);

            //If we successfully read the data in.
            if (contents != null)
            {
                //Loop through every line.
                foreach (string line in contents)
                {
                    //Detect the line type. (Only passing in first character, less expensive than passing in whole string.)
                    if (!string.IsNullOrEmpty(line))
                    {
                        InixType type = detectType(line[0]);

                        parseLine(type, line);
                    }
                }

                InixLogger.log("Finished parsing the file.");
            }

            return inixFile;
        }

        private void parseLine(InixType type, string line)
        {
            switch (type)
            {
                case InixType.Property:

                    //Split property, value and comment.
                    string[] commentSplit = line.Split(new[] { ';' }, 2);

                    string[] propertySplit = commentSplit[0].Split(new[] { '=' }, 2);

                    if(propertySplit.Length != 2)
                    {
                        inixFile.errors.Add($"There was an error parsing the property - [{line}] {propertySplit.Length}");
                    }
                    else
                    {
                        //InixLogger.log($"Processing -> [ {line} ], commentSplit: {commentSplit.Length} | propertySplit: {propertySplit.Length}");

                        InixProperty property = new(propertySplit[1].Trim(), (commentSplit.Length > 1) ? commentSplit[1].Trim() : "");

                        //Get the last header and add it.
                        if (inixFile.inixObjects.ContainsKey(lastHeader))
                        {
                            inixFile.inixObjects[lastHeader].properties.Add(propertySplit[0], property);
                        }
                    }

                    break;
                case InixType.Header:

                    InixObject headerObject = new(type);

                    //The idea is that we keep the brackets to avoid the names of comments interfering with header names.
                    //When they goto access the dictionary for e.g. Instance["someKey"], it will add brackets to the start and end
                    //We are taking advantage of insertion order here.

                    //Check if we have a ";"
                    string[] headerSplit = line.Split(new[] { ';' }, 2);

                    if (headerSplit.Length > 1)
                    {
                        headerObject.comment = headerSplit[1].Trim(); //If there is a header after the "]", then this will add it.
                    }

                    string header = headerSplit[0].Trim();

                    if (header[header.Length - 1] != ']')
                    {
                        inixFile.errors.Add($"There was an error parsing header - {header} -> It is missing a closing bracket.");
                    }
                    else
                    {
                        lastHeader = header;

                        inixFile.inixObjects.Add(header, headerObject);
                    }

                    break;
                case InixType.Comment:

                    //Increment the comment count.
                    commentCount++;

                    inixFile.inixObjects.Add($"Comment-{commentCount}", new InixObject(type, line));

                    break;
            }
        }

        private void cleanup()
        {
            contents = null;
            commentCount = 0;
            lastHeader = "";

            if (inixFile != null)
                inixFile = new();
        }

        private InixType detectType(char firstChar)
        {
            switch (firstChar)
            {
                //Header
                case '[':
                    return InixType.Header;
                //Comment <- Specific To Assetto Corsa
                case '/':
                    return InixType.Comment;
                //Normal Comment
                case ';':
                    return InixType.Comment;
                //If anything else, it is a property.
                default:
                    return InixType.Property;
            }
        }

        private void loadFile(string path)
        {
            InixLogger.log($"Loading in file with path - {path}");

            try
            {
                contents = File.ReadAllLines(path);

                InixLogger.log($"Loaded {contents.Length} lines into array.");
            }
            catch (Exception ex)
            {
                inixFile.errors.Add($"There was an error reading the file - {ex.Message}");

                InixLogger.log($"Error reading lines of file - {ex.Message}");
            }
        }
    }
}