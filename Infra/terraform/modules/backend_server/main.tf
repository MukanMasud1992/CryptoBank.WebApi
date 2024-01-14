resource "hcloud_firewall" "backend_firewall" {
    name = "${var.name}_firewal"
    
    rule {
      destination_ips = []
      direction = "in"
      port = "22"
      protocol = "tcp"
      source_ips = [
        "0.0.0.0/0",
        "::/0",
      ]
    }

    rule {
      direction = "out"
      port = "53"
      protocol = "tcp"
      destination_ips = [
        "0.0.0.0/0",
        "::/0",
      ]
    }

    rule {
      direction = "out"
      port = "53"
      protocol = "udp"
         destination_ips = [
        "0.0.0.0/0",
        "::/0",
      ]
    }

    rule {
      direction = "out"
      port = "80"
      protocol = "tcp"
         destination_ips = [
        "0.0.0.0/0",
        "::/0",
      ]
    }

        rule {
      direction = "out"
      port = "80"
      protocol = "udp"
         destination_ips = [
        "0.0.0.0/0",
        "::/0",
      ]
    }

            rule {
      direction = "out"
      port = "443"
      protocol = "tcp"
         destination_ips = [
        "0.0.0.0/0",
        "::/0",
      ]
    }

            rule {
      direction = "out"
      port = "443"
      protocol = "udp"
         destination_ips = [
        "0.0.0.0/0",
        "::/0",
      ]
    }
}

resource "hcloud_server" "backend" {
  name = var.name
  server_type = var.server_type
  image = var.image
  location = var.location
  ssh_keys = var.ssh_keys

  network {
    network_id = var.network_id
  }

  firewall_ids = [ hcloud_firewall.backend_firewall.id ]

  labels = {
    purpose = "backend"
  }
}