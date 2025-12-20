namespace TheKittySaver.AdoptionSystem.API.QueriesSorting;

internal static class SortParser
{
    extension(IEnumerable<SortItem>)
    {
        public static IEnumerable<SortItem> Parse(string sort)
        {
            if (string.IsNullOrWhiteSpace(sort))
            {
                yield break;
            }

            string[] lines = sort.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();
                string[] parts = trimmedLine.Split(':', 2);

                string propertyName = parts[0].Trim();
                if (string.IsNullOrEmpty(propertyName))
                {
                    continue;
                }

                SortOperand operand =
                    parts.Length > 1 && parts[1].Trim().Equals("desc", StringComparison.OrdinalIgnoreCase)
                        ? SortOperand.Desc
                        : SortOperand.Asc;
                SortItem sortItem = new(propertyName, operand);
                yield return sortItem;
            }
        }
    }
}
