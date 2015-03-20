namespace BetaSeriesW8.Service
{
    public static class TraductionErreur
    {
        public static string RecupererErreur(int codeErreur)
        {
            switch (codeErreur)
            {
                case 1001:
                    return "Clé API invalide";
                case 1002:
                    return "Type invalide";
                case 1003:
                    return "Action invalide";
                case 2001:
                    return "Token utilisateur invalide.";
                case 2002:
                    return "Les réglages vie privée de l'utilisateur ne permettent pas l'action";
                case 2003:
                    return "La série est déjà dans le compte utilisateur";
                case 2004:
                    return "La série n'est pas dans le compte utilisateur";
                case 2005:
                    return "L'utilisateur n'a pas vu cet épisode";
                case 2006:
                    return "Les deux utilisateurs ne sont pas amis";
                case 2007:
                    return "Les options de l'utilisateur ne sont pas valides";
                case 3001:
                    return "Tous les champs doivent être remplis";
                case 3002:
                    return "Le terme doit avoir au moins 2 caractères";
                case 3003:
                    return "Le paramètre doit être un nombre";
                case 3004:
                    return "Valeur de la variable incorrecte";
                case 3005:
                    return "Caractères non autorisés";
                case 3006:
                    return "Adresse e-mail invalide";
                case 4001:
                    return "La série n'existe pas";
                case 4002:
                    return "L'utilisateur n'existe pas";
                case 4003:
                    return "Mauvais mot de passe";
                case 4004:
                    return "L'utilisateur existe déjà";
                default:
                    return string.Empty;
            }
        }
    }
}