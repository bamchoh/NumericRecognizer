using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NumericRecognizer
{
    public class ItemViewModelDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var element = container as FrameworkElement;
            var viewModel = item as CommandBase;
            if (element == null || viewModel == null) return null;

            var templateName = viewModel.GetType().Name + "Template";

            return element.FindResource(templateName) as DataTemplate;
        }
    }
}
