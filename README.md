# Inix

A lightweight C# ini parser:

- ~250 lines of code.
- Pure C# code, only 1 import to use StringBuilder.
- Built in **.NET 6.0**
- 11KB in size.

## Support

Below defines what is supported by inix.

```ini
; global comments
// comments starting with a slash

[HEADER] ; comments on headers.
TESTING=0 ; comments on properties.

; and obviously being able to access each header, its comments. As well as each property and its value/comments.

```

## Motive

I personally needed a highly customizable ini parser for the AssettoTools project, other solutions were bloated in my opinion and the same results can be achieved in a quarter of the code as seen in this repository.

## Usage

The usage is demonstrated below, however, there is an examples solution ```Inix.Examples``` that contains the same information.

Note: inix.parse takes a **file path**, not contents. Will most likely add another method for parsing raw data.
The [CAMBER_RF] and [MIN] snippets below are referring to the ```Inix.Examples\\Data\\test.ini``` file. It is included in the repository so you can try it for yourself aswell.

The **InixLoader** is a class you only need to instantiate once, it contains all of the code needed to parse the INI file and return an **InixFile**. The **InixFile** is an object that houses all of the information regarding an INI file. All information sorrounding how to use the **InixFile** is listed below in the code snippet.

```c#
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
```


