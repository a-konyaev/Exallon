namespace Exallon.Configurations
{
    public interface IConfigurationManager : ISubsystem, IConfigurationService
    {
        /// <summary>
        /// Получить описание отображения справочника
        /// </summary>
        /// <param name="catalogName">имя справочника</param>
        /// <returns></returns>
        CatalogView GetCatalogView(string catalogName);

        /// <summary>
        /// Получить описание отображения документа
        /// </summary>
        /// <param name="documentName">имя документа</param>
        /// <returns></returns>
        DocumentView GetDocumentView(string documentName);
    }
}
