using System.Text;

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

    public class InixObject
    {
        public InixObject(InixType type)
        {
            this.type = type;

            properties = new();
        }

        public InixObject(InixType type, string rawData)
        {
            this.type = type;
            this.rawData = rawData;
        }

        public InixType type { get; set; } = InixType.Comment;

        public Dictionary<string, InixProperty>? properties { get; set; } = null;

        public string rawData { get; set; } = "";

        public InixProperty this[string key]
        {
            get { return properties[key]; }
            set { properties[key] = value; }
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

        private Dictionary<string, InixObject> inixObjects = new();

        public InixObject this[string key]
        {
            get { return inixObjects[cleanseKey(key)]; }
            set { inixObjects[cleanseKey(key)] = value; }
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
                        string headerComment = (string.IsNullOrEmpty(objects.Value.rawData)) ? "" : " ; " + objects.Value.rawData;

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
                        returnObject.AppendLine(objects.Value.rawData);
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

        public bool parse(string path)
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

                return true;
            }
            else
            {
                return false;
            }
        }

        public int objectCount()
        {
            return inixObjects.Count();
        }

        public bool containsHeader(string key)
        {
            return inixObjects.ContainsKey(cleanseKey(key));
        }

        private void parseLine(InixType type, string line)
        {
            switch (type)
            {
                case InixType.Property:

                    //Split property, value and comment.
                    string[] commentSplit = line.Split(';');

                    string[] propertySplit = commentSplit[0].Split('=');

                    //InixLogger.log($"Processing -> [ {line} ], commentSplit: {commentSplit.Length} | propertySplit: {propertySplit.Length}");

                    InixProperty property = new(propertySplit[1].Trim(), (commentSplit.Length > 1) ? commentSplit[1].Trim() : "");

                    //Get the last header and add it.
                    if (inixObjects.ContainsKey(lastHeader))
                    {
                        inixObjects[lastHeader].properties.Add(propertySplit[0], property);
                    }

                    break;
                case InixType.Header:

                    InixObject headerObject = new(type);

                    //The idea is that we keep the brackets to avoid the names of comments interfering with header names.
                    //When they goto access the dictionary for e.g. Instance["someKey"], it will add brackets to the start and end
                    //We are taking advantage of insertion order here.

                    //Check if we have a ";"
                    string[] headerSplit = line.Split(';');

                    if (headerSplit.Length > 1)
                    {
                        headerObject.rawData = headerSplit[1].Trim(); //If there is a header after the "]", then this will add it.
                    }

                    lastHeader = headerSplit[0].Trim();

                    inixObjects.Add(headerSplit[0].Trim(), headerObject);

                    break;
                case InixType.Comment:

                    //Increment the comment count.
                    commentCount++;

                    inixObjects.Add($"Comment-{commentCount}", new InixObject(type, line));

                    break;
            }
        }

        public void printDictionary()
        {
            foreach (KeyValuePair<string, InixObject> keyVal in inixObjects)
            {
                InixLogger.log($"{keyVal.Key} -> {keyVal.Value.type}");
            }
        }

        private void cleanup()
        {
            contents = null;
            commentCount = 0;
            lastHeader = "";

            if (inixObjects.Count > 0)
                inixObjects.Clear();
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
            }

            //If anything else, it is a property.
            return InixType.Property;
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
                InixLogger.log($"Error reading lines of file - {ex.Message}");
            }
        }
    }
}