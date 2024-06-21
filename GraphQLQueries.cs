namespace TarkovDataOrganizer;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;


public static class GraphQueries
{

    private const string TARKOV_API_ENDPOINT = "https://api.tarkov.dev/graphql";
    public static async Task<GraphQLResponse<dynamic>> QueryTarkovAPI(string _query)
    {
        var graphQlClient = new GraphQLHttpClient(TARKOV_API_ENDPOINT, new NewtonsoftJsonSerializer()); 
        var graphQlRequest = new GraphQLRequest
        {
            Query = _query
        };

        var response = await graphQlClient.SendQueryAsync<dynamic>(graphQlRequest); 
        return response;
    }

    public const string QUERY_MASTER_ITEMS_ALL =
        @"query {
            items
               {
                id
                name
                normalizedName
                shortName
                description
                categories { id name normalizedName }
                category { id name normalizedName }
                bsgCategoryId
                handbookCategories { id name normalizedName }
                types
                updated
                
                usedInTasks { 
                  id
                	name
                  trader { name }
                  taskRequirements { task { name } status }
                	traderRequirements {
                    id
                    trader {name}
                    requirementType
                    compareMethod
                    value
                  }
                  objectives {
                    id
                    type
                    description
                    optional
                  }
                  finishRewards {
                    items { item { id name } }
                  }
                  restartable
                  factionName
                  kappaRequired
                  lightkeeperRequired
                  descriptionMessageId
                  startMessageId
                  successMessageId
                  failMessageId
                
                }
                
                receivedFromTasks { 
                  id
                	name
                  trader { name }
                  taskRequirements { task { name } status }
                	traderRequirements {
                    id
                    trader {name}
                    requirementType
                    compareMethod
                    value
                  }
                  objectives {
                    id
                    type
                    description
                    optional
                  }
                  finishRewards {
                    items { item { id name } }
                    craftUnlock { rewardItems { item { name }}}
                    offerUnlock { trader { name } item { name } level }
                    traderUnlock { name levels { level } }
                    skillLevelReward {skill { name } level name }
                    traderStanding { trader { name } standing }
                    
                  }
                  restartable
                  factionName
                  kappaRequired
                  lightkeeperRequired
                  descriptionMessageId
                  startMessageId
                  successMessageId
                  failMessageId
                
                }

                weight
                ergonomicsModifier
                accuracyModifier
                recoilModifier
                loudness
                velocity
                
                blocksHeadphones
                conflictingSlotIds
                conflictingItems
                {
                  id
                  name
                }
                
                hasGrid
                width
                height
                backgroundColor
                gridImageLink
                
                link
                iconLink                
                inspectImageLink
                image8xLink
                image512pxLink
                baseImageLink
                wikiLink
                
                basePrice
                avg24hPrice
                low24hPrice
                lastLowPrice
                high24hPrice
                changeLast48h
                changeLast48hPercent
                lastOfferCount
                sellFor {
                  vendor {
                    name
                    normalizedName
                  }
                  price
                  priceRUB
                  currency
                  currencyItem { id name }
                  vendor { name }
                }
                buyFor {
                  price
                  priceRUB
                  currency
                  currencyItem { id name }
                  vendor { name }
                  
                }
                bartersFor {
                  id
                  buyLimit
                  trader { id name }
                  level
                  taskUnlock { id name }
                  rewardItems { item { id name }}
                  requiredItems { item { id name }}
                }
                bartersUsing {
                  id
                  buyLimit
                  trader { id name }
                  level
                  taskUnlock { id name }
                  rewardItems { item { id name }}
                  requiredItems { item { id name }}
                }
                
                  
                  
                
                containsItems {
                  item { name }
                  count
                  quantity
                  attributes { type name value }
                }
                                
                properties {
                  ... on ItemPropertiesWeapon
                  {        
                    caliber
                    
                    ergonomics
                    
                    fireRate
                    fireModes
                    
                    recoilVertical
                    recoilHorizontal
                    
                    defaultPreset {
                      id
                      name
                      baseImageLink
                      gridImageLink
                      image8xLink
                      image512pxLink
                    }
                    
                    presets {
                      id
                      name
                    }
                    
                    slots {
                      id
                      nameId
                      required
                      name
                      filters {
                        allowedItems { id name }
                        excludedItems { id name }
                        allowedCategories { id name }
                        excludedCategories { id name }
                      }
                    }
                  }
                  
                  ... on ItemPropertiesWeaponMod
                    {                        
                      slots {
                        id
                        nameId
                        required
                        name
                        filters {
                          allowedItems { id name }
                          excludedItems { id name }
                          allowedCategories { id name }
                          excludedCategories { id name }
                        }
                      }
                    }
                  ... on ItemPropertiesBarrel
                  {
                    ergonomics
                    recoilModifier
                    centerOfImpact
                    deviationCurve
                    deviationMax
                    
                    slots {
                        id
                        nameId
                        required
                        name
                        filters {
                          allowedItems { id name }
                          excludedItems { id name }
                          allowedCategories { id name }
                          excludedCategories { id name }
                        }
                      }
                  }
                }
              }
            }
        ";

    public const string QUERY_ITEMS_ALL_GENERIC_INFO =
      @"";

    public const string QUERY_TRADER_CASH_OFFERS =
      @"query {
        traders
        {
          name
          cashOffers
          {
            item { id name }
            minTraderLevel
            price
            currency
            currencyItem { name }
            priceRUB
            taskUnlock { id name trader { id name } minPlayerLevel }
            buyLimit
          }
        }
      }";

    public const string QUERY_BARTERS =
      @"query {
        barters
        {
			      id    
    	      level
    	      trader { id name }
    	      requiredItems {
              item { id name } 
              count 
              quantity 
            }
    	      rewardItems {
              item { id name } 
              count 
              quantity 
            }
            taskUnlock { id name trader { id name } minPlayerLevel }
            buyLimit
          }
      }";
}