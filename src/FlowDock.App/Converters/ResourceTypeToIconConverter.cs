namespace FlowDock.App.Converters;

public class ResourceTypeToIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is ResourceType type)
        {
            return type switch
            {
                ResourceType.Application => "",  // application window
                ResourceType.Folder => "",         // folder
                ResourceType.URL => "",             // globe
                ResourceType.SystemAction => "",    // settings
                ResourceType.Workflow => "",        // play flow
                _ => ""
            };
        }

        return "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}
