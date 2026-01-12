using G_StockVente.Domain.Models;
using Microsoft.AspNetCore.Http;

namespace G_StockVente.Infrastructure.Services;

/// <summary>
/// Service pour gérer le panier de vente en session
/// </summary>
public class PanierService
{
    private const string SessionKey = "PanierVente";

    public static List<LignePanier> GetPanier(ISession session)
    {
        var panierJson = session.GetString(SessionKey);
        if (string.IsNullOrEmpty(panierJson))
        {
            return new List<LignePanier>();
        }

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<LignePanier>>(panierJson) 
                ?? new List<LignePanier>();
        }
        catch
        {
            return new List<LignePanier>();
        }
    }

    public static void SavePanier(ISession session, List<LignePanier> panier)
    {
        var panierJson = System.Text.Json.JsonSerializer.Serialize(panier);
        session.SetString(SessionKey, panierJson);
    }

    public static void ClearPanier(ISession session)
    {
        session.Remove(SessionKey);
    }
}

public class LignePanier
{
    public Guid ProduitBoutiqueId { get; set; }
    public string NomProduit { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public int Quantite { get; set; }
    public decimal PrixUnitaire { get; set; }
    public decimal Remise { get; set; }
    public decimal TauxTVA { get; set; }
    public decimal MontantHT { get; set; }
    public decimal MontantTVA { get; set; }
    public decimal MontantTTC { get; set; }
}

