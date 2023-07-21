public async Task<JsonResult> getPrice(int idcountry, DateTime startdate, DateTime enddate, int idtriptype, int idplan, int familymember)
        {
            string debugCrypt = "";

            var plan = _Scontext.Plan.Where(i => i.IdPlan == idplan && i.Status == "A").Single();
            var tripType = _Scontext.TripType.Where(i => i.IdTripType == idtriptype && i.Status == "A").Single();
            var planCategory = _Scontext.PlanCategory.Where(i => i.IdPlanCategory == plan.IdPlanCategory).Single();
            var toEncrypt = "";
            var currency = _Scontext.Currency.Where(i => i.IdCurrency == plan.IdCurrency && i.Status == "A").Single();
            var region = "";
            var valueId11 = "";

            if (planCategory.PlanCategoryName == "INTERNATIONAL")
            {
                var countryInternational = _Scontext.Country.Where(i => i.IdCountry == idcountry && i.Status == "A").Single();
                var regionInternational = _Scontext.Region.Where(i => i.IdRegion == countryInternational.IdRegion && i.Status == "A").Single();
                region = regionInternational.RegionName;
                valueId11 = plan.PlanName.ToUpper();
            }
            else if (planCategory.PlanCategoryName == "DOMESTIC")
            {
                valueId11 = plan.CoverageID.ToUpper();
                //idcountry = 96;
                //var countryDomestik = _Scontext.Country.Where(i => i.IdCountry == idcountry).Single();
                //var regionDomestik = _Scontext.Region.Where(i => i.IdRegion == countryDomestik.IdRegion).Single();
                region = "WORLDWIDE";
            }

            var coverageCode2 = plan.CoverageCode2;
            if (coverageCode2 == null)
            {
                coverageCode2 = "";
            }
            else
            {
                coverageCode2 = coverageCode2.ToUpper();
            }

            APIAction = "PremiumSimulation";

            var premiumSimulation = new PremiumSimulationViewModel
            {
                ProductID = "1217",
                CoverageID = plan.CoverageID.ToUpper(),
                IncludeExtCovF = "true",
                InceptionDate = startdate.ToString("yyyy-MM-dd"),
                ExpiryDate = enddate.ToString("yyyy-MM-dd"),
                Currency = currency.CurrencyCode,
                SI_1 = "1",
                SI_2 = "1",
                SI_3 = "0",
                SI_4 = "0",
                SI_5 = "0",
                FLDID1 = "A01",
                ValueID1 = "Dummy-Address",
                FLDID2 = "I11",
                ValueID2 = "1234123412341234",
                FLDID3 = "B02",
                ValueID3 = "2000-01-20",
                FLDID4 = "B10",
                ValueID4 = "LEGALHEIRS",
                FLDID5 = "R01",
                ValueID5 = "Policy Holder",
                FLDID6 = "D10",
                ValueID6 = startdate.ToString("yyyy-MM-dd"),
                FLDID7 = "D11",
                ValueID7 = enddate.ToString("yyyy-MM-dd"),
                FLDID8 = "D12",
                ValueID8 = (enddate - startdate).Days.ToString(),
                FLDID9 = "T21",
                ValueID9 = tripType.TripTypeName.ToUpper(),
                FLDID10 = "T22",
                ValueID10 = planCategory.PlanCategoryName.ToUpper(),
                FLDID11 = "T23",
                //planname inter
                ValueID11 = valueId11,
                FLDID12 = "D13",
                ValueID12 = region.ToUpper(),
                FLDID13 = "D14",
                ValueID13 = "DUMMY-COUNTRY",
                FLDID14 = "T24",
                ValueID14 = "SINGLE TRIP",
                FLDID15 = "T12",
                ValueID15 = "-",
                ASource = "MPM_POLICY",
                //CoverageCode1 = plan.CoverageCode1.ToUpper(),
                //CoverageCode2 = coverageCode2,
                CoverageCode1 = "",
                CoverageCode2 = "",
                CoverageCode3 = "",
                CoverageCode4 = "",
                CoverageCode5 = "",
                CoverageCode6 = "",
                CoverageCode7 = "",
                CoverageCode8 = "",
                CoverageCode9 = "",
                CoverageCode10 = "",
                PolicyDetail = new List<PolicyDetail> { new PolicyDetail { } },
                Beneficiaries = new List<Beneficiaries> { new Beneficiaries { } },
                InterestedParty = new List<InterestedParty> { new InterestedParty { } }
            };

            toEncrypt = JsonConvert.SerializeObject(premiumSimulation);

            //toEncrypt = "{" +
            //    "\"ProductID\":\"1217\"," +
            //    "\"CoverageID\":\"" + plan.CoverageID.ToUpper() + "\"," +
            //    "\"IncludeExtCovF\":\"true\"," +
            //    "\"InceptionDate\":\"" + startdate.ToString("yyyy-MM-dd") + "\"," +
            //    "\"ExpiryDate\":\"" + enddate.ToString("yyyy-MM-dd") + "\"," +
            //    "\"Currency\":\"USD\"," +
            //    "\"SI_1\":\"29400\"," +
            //    "\"SI_2\":\"0\"," +
            //    "\"SI_3\":\"0\"," +
            //    "\"SI_4\":\"0\"," +
            //    "\"SI_5\":\"0\"," +
            //    "\"FLDID1\":\"A01\"," +
            //    "\"ValueID1\":\"DUMMY-ADDRESS\"," +
            //    "\"FLDID2\":\"I11\"," +
            //    "\"ValueID2\":\"1234123412341234\"," +
            //    "\"FLDID3\":\"B02\"," +
            //    "\"ValueID3\":\"2000-01-20\"," +
            //    "\"FLDID4\":\"B10\"," +
            //    "\"ValueID4\":\"LEGALHEIRS\"," +
            //    "\"FLDID5\":\"R01\"," +
            //    "\"ValueID5\":\"Policy Holder\"," +
            //    "\"FLDID6\":\"D10\"," +
            //    "\"ValueID6\":\"" + startdate.ToString("yyyy-MM-dd") + "\"," +
            //    "\"FLDID7\":\"D11\"," +
            //    "\"ValueID7\":\"" + enddate.ToString("yyyy-MM-dd") + "\"," +
            //    "\"FLDID8\":\"D12\"," +
            //    "\"ValueID8\":\"" + (enddate - startdate).Days.ToString() + "\"," +
            //    "\"FLDID9\":\"T21\"," +
            //    "\"ValueID9\":\"" + tripType.TripTypeName.ToUpper() + "\"," +
            //    "\"FLDID10\":\"T22\"," +
            //    "\"ValueID10\":\"INTERNATIONAL\"," +
            //    "\"FLDID11\":\"T23\"," +
            //    "\"ValueID11\":\"" + plan.PlanName.ToUpper() + "\"," +
            //    "\"FLDID12\":\"D13\"," +
            //    "\"ValueID12\":\"" + region.RegionName.ToUpper() + "\"," +
            //    "\"FLDID13\":\"D14\"," +
            //    "\"ValueID13\":\"DUMMY-COUNTRY\"," +
            //    "\"FLDID14\":\"T24\"," +
            //    "\"ValueID14\":\"SINGLE TRIP\"," +
            //    "\"FLDID15\":\"T12\"," +
            //    "\"ValueID15\":\"-\"," +
            //    "\"ASource\":\"MPM_POLICY\"," +
            //    "\"CoverageCode1\":\"TRAVEL-01\"," +
            //    "\"CoverageCode2\":\"\"," +
            //    "\"CoverageCode3\":\"\"," +
            //    "\"CoverageCode4\":\"\"," +
            //    "\"CoverageCode5\":\"\"," +
            //    "\"CoverageCode6\":\"\"," +
            //    "\"CoverageCode7\":\"\"," +
            //    "\"CoverageCode8\":\"\"," +
            //    "\"CoverageCode9\":\"\"," +
            //    "\"CoverageCode10\":\"\"," +
            //    "\"PolicyDetail\":[]," +
            //    "\"Beneficiaries\":[]," +
            //    "\"InterestedParty\":[]" +
            //    "}";

            var ResultString = await Encrypt(toEncrypt, APIAction, false);
            debugCrypt = debugCrypt + "\n\nAPI Action:\n" + APIAction + "\n\nEncryption Data:\n" + toEncrypt + "\n\nResult String:\n" + ResultString;

            var rate = "";

            try
            {
                var resultProfile = JsonConvert.DeserializeObject<ResponseAPI>(ResultString);
                DeserializedString = "Status: " + resultProfile.status + "\n Code: " + resultProfile.code + "\n Message: " + resultProfile.message + "\n Data: " + resultProfile.Data.First().Rate;

                //var DeserializedData = resultProfile.Data.First();
                //rate = DeserializedData.Rate;

                var DeserializedData = resultProfile.Data.ToList();
                var parentRate = DeserializedData.Where(i => i.Code == "TRAVEL-01").Single().Rate;
                decimal rateNewRate;
                decimal rateParentRate;
                decimal count;

                if (plan.PlanDisplayName == "Platinum Covid")
                {
                    var newRate = DeserializedData.Where(i => i.Code.ToUpper() == "TRAVCOVPLA").Single().Rate;
                    rateNewRate = decimal.Parse(newRate, CultureInfo.InvariantCulture);
                    rateParentRate = decimal.Parse(parentRate, CultureInfo.InvariantCulture);

                    if (familymember > 0)
                    {
                        count = (rateNewRate * (familymember + 1)) + rateParentRate;
                        rate = count.ToString(CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        count = rateNewRate + rateParentRate;
                        rate = count.ToString(CultureInfo.InvariantCulture);
                    }

                }
                else if (plan.PlanDisplayName == "Diamond Covid")
                {
                    var newRate = DeserializedData.Where(i => i.Code.ToUpper() == "TRAVCOVDIA").Single().Rate;
                    rateNewRate = decimal.Parse(newRate, CultureInfo.InvariantCulture);
                    rateParentRate = decimal.Parse(parentRate, CultureInfo.InvariantCulture);

                    if (familymember > 0)
                    {
                        count = (rateNewRate * (familymember + 1)) + rateParentRate;
                        rate = count.ToString(CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        count = rateNewRate + rateParentRate;
                        rate = count.ToString(CultureInfo.InvariantCulture);
                    }
                }
                else
                {
                    rate = parentRate;
                }

                ViewData["DataEnkrip"] = rate;
                ViewData["ResponseStringProfile"] = responseString;
            }
            catch (Exception e)
            {
                DeserializedString = e.ToString();
            }



            var testList = new List<SelectListItem>();
            testList.Add(new SelectListItem { Text = rate, Value = rate });

            LogData(debugCrypt, "getPrice", "TransactionController");

            return Json(testList);
        }