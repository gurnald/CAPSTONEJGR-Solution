namespace CAPSTONEJGR.Models;

public class Po {

    public Vendor Vendor { get; set; }
    public IEnumerable<PoLine> PoLines { get; set; }
    public decimal PoTotal { get; set; }


}
