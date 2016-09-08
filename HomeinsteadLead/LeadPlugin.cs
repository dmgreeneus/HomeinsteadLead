using System;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;



namespace HomeinsteadLead
{
    public class LeadPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracingService =
               (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginExecutionContext context =
                (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity lead = (Entity)context.InputParameters["Target"];
                if (lead.LogicalName != "lead")
                    return;
                String statevalue = null;
                try
                {
                    if (lead.Attributes.Contains("hisc_state"))
                    {
                        if (lead.GetAttributeValue<String>("hisc_state") != null)
                        {
                            statevalue = lead.GetAttributeValue<String>("hisc_state");
                            Entity state = new Entity();
                            state.LogicalName = "hisc_state";
                            QueryExpression query = new QueryExpression();
                            if (statevalue.Length > 2)
                            {

                                query = new QueryExpression
                                {
                                    EntityName = state.LogicalName,
                                    ColumnSet = new ColumnSet("hisc_stateid"),
                                    Criteria = new FilterExpression
                                    {
                                        FilterOperator = LogicalOperator.And,
                                        Conditions =
                                    {
                                        new ConditionExpression
                                        {
                                            AttributeName = "hisc_name",
                                            Operator = ConditionOperator.Equal,
                                            Values = { statevalue }
                                        }
                                   }
                                    }
                                };

                            }

                            else if (statevalue.Length == 2)
                            {
                                query = new QueryExpression
                                {
                                    EntityName = state.LogicalName,
                                    ColumnSet = new ColumnSet("hisc_stateid"),
                                    Criteria = new FilterExpression
                                    {
                                        FilterOperator = LogicalOperator.And,
                                        Conditions =
                                    {
                                            new ConditionExpression
                                            {
                                                AttributeName = "hisc_abbreviation",
                                                Operator = ConditionOperator.Equal,
                                                Values = { statevalue }
                                            }
                                    }
                                    }
                                };

                            }

                            if (query != null)
                            {
                                EntityCollection results = service.RetrieveMultiple(query);
                                foreach (var result in results.Entities)
                                {
                                    Guid stateid = result.GetAttributeValue<Guid>("hisc_stateid");
                                    lead.Attributes["hisc_stateprovinceid"] = new EntityReference(state.LogicalName, stateid);

                                }
                            }

                        }
                    }
                }
                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException("an error occured in lead plugin", ex);
                }
                catch (Exception ex)
                {
                    tracingService.Trace("HISC_Lead_Pluign: {0}", ex.Message.ToString());
                    throw;

                }

            }
        }
    }
}

