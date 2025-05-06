-- Line below loads our DLL
require ("NetQuikConnector")
-- For debug:  NetQuikConnector.Test("Init done")

IsStop = false; 
 
function main()
   local FirstQuote = true;
   local Quote = "";

   while not IsStop do
      sleep(1000);
        -- Quik lua engine really does not like to collect garbage, so we`ll force it
      collectgarbage();
   end;

end

-- Called friom Quik on new Quote
-- Depth of market window Should be open in Quik
function OnQuote(class, sec )

    if class == nil or sec == nil then
        return
    end

    q = getQuoteLevel2(class, sec);
      
    local parts = {}
    table.insert(parts, os.time())
    table.insert(parts, sec)
    table.insert(parts, class)
    
    -- Process bids
    for i = tonumber(q.bid_count), 1, -1 do
        table.insert(parts, q.bid[i].quantity or "0")
        table.insert(parts, tostring(tonumber(q.bid[i].price)))
    end
    
	--This guarantees separation beteween bid and asks
    table.insert(parts, "|")
    
    -- Process asks
    for i = tonumber(q.offer_count), 1, -1 do
        table.insert(parts, q.offer[i].quantity or "0")
        table.insert(parts, tostring(tonumber(q.offer[i].price)))
    end
    
    local qs = table.concat(parts, ";");
    -- Call c# Method...
    NetQuikConnector.NewQuote(qs)
  
end;
 
--Called from Quik
function OnStop(s)
   IsStop = true;
end