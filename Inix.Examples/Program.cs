namespace Inix.Examples
{
    public class Program
    {
        public Program()
        {
            //Example1();
            BrokenExample();

            Console.ReadLine();
        }

        public void BrokenExample()
        {
            //Enables or disables the logging, users choice.
            InixLogger.shouldLog = true;

            //Make new inix instance.
            InixLoader inix = new();

            //Pass the path into inix and parse the INI file.
            InixFile result = inix.parse("Data\\broken.ini");

            if (result.hasErrors)
            {
                Console.WriteLine("There was an error reading the INI file.");

                //Print out the errors
                for (int i = 0; i < result.errors.Count; i++)
                {
                    Console.WriteLine($"[{i}] -> {result.errors[i]}");
                }
            }
            else
            {
                Console.WriteLine("The parsing was successful.");
            }
        }

        public void Example1()
        {
            //Enables or disables the logging, users choice.
            InixLogger.shouldLog = true;

            //Make new inix instance.
            InixLoader inix = new();

            //Pass the path into inix and parse the INI file.
            InixFile result = inix.parse("Data\\test.ini");

            //If the parsing was successful.
            if (!result.hasErrors)
            {
                Console.WriteLine("The parsing was successful.");

                //Print each header and comment of the dictionary
                result.printDictionary();

                //Enumerating through the properties of a header.
                foreach (KeyValuePair<string, InixProperty> keyVal in result["[CAMBER_RF]"].properties)
                {
                    Console.WriteLine($"[{keyVal.Key}] -> {keyVal.Value.value} | Comment: {keyVal.Value.comment}");
                }

                //Print individual property out, testing [CAMBER_RF][MIN]
                InixProperty propertyData = result["[CAMBER_RF]"]["MIN"];

                Console.WriteLine($"[MIN] -> {propertyData.value} -> {propertyData.comment}");

                //Reconstruct the ini
                string reconstructed = result.ToString();

                Console.WriteLine(reconstructed);
            }
            else
            {
                Console.WriteLine("There was an error reading the INI file.");
            }

        }

        private static void Main(string[] args)
        {
            Program p = new();
        }
    }
}