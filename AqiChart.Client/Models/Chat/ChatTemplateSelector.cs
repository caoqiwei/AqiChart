using System.Windows.Controls;
using System.Windows;

namespace AqiChart.Client.Models.Chat
{
    public class ChatTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            ChatContent content = item as ChatContent;
            if (content != null)
            {
                DataTemplate template;
                //判定数据实体类型选择不同的数据模板
                if (!content.IsMe)
                {
                    template = Application.Current.TryFindResource("chatother") as DataTemplate;
                }
                else
                {
                    template = Application.Current.TryFindResource("chatowner") as DataTemplate;
                }
                return template;
            }

            return null;
        }
    }
}
