namespace Inix.Examples
{
    public class Program
    {
        public Program()
        {
            Example1();

            Console.ReadLine();
        }

        public void Example1()
        {
            //Enables or disables the logging, users choice.
            InixLogger.shouldLog = true;

            //Make new inix instance.
            InixLoader inix = new();

            //Pass the path into inix and parse the INI file.
            bool result = inix.parse("Data\\test.ini");

            //If the parsing was successful.
            if (result)
            {
                Console.WriteLine("The parsing was successful.");

                //Print each header and comment of the dictionary
                inix.printDictionary();

                //Enumerating through the properties of a header.
                foreach (KeyValuePair<string, InixProperty> keyVal in inix["[CAMBER_RF]"].properties)
                {
                    Console.WriteLine($"[{keyVal.Key}] -> {keyVal.Value.value} | Comment: {keyVal.Value.comment}");
                }

                //Print individual property out, testing [CAMBER_RF][MIN]
                InixProperty propertyData = inix["[CAMBER_RF]"]["MIN"];

                Console.WriteLine($"[MIN] -> {propertyData.value} -> {propertyData.comment}");

                //Reconstruct the ini
                string reconstructed = inix.ToString();

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