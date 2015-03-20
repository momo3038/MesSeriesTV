using Windows.Data.Json;

namespace BetaSeriesW8.Service
{
    public static class ServiceErreur
    {
        public static void LeverExceptionSiErreur(JsonObject root)
        {
            var codeErreur = RecupererCodeErreur(root);
            if (codeErreur != null)
            {
                throw new BetaSerieException(codeErreur.Value);
            }
        }

        public static int? RecupererCodeErreur(JsonObject root)
        {
            var erreurs = root.GetNamedObject("errors");
            if (erreurs.Keys.Count > 0)
            {
                int? codeErreur = null;
                foreach (var erreur in erreurs.Keys)
                {
                    codeErreur = (int) erreurs.GetNamedObject(erreur).GetNamedNumber("code");
                }
                return codeErreur;
            }
            return null;
        }
    }
}