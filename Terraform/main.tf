terraform {
  required_providers {
    yandex = {
      source = "yandex-cloud/yandex"
    }
  }
  required_version = ">= 0.13"
}

variable "ycToken" {
  type = string
  sensitive = true
}

provider "yandex" {
  token     = var.ycToken
  cloud_id  = "b1gehbv35gtkepghrc6l"
  folder_id = "b1gg3e45d51c39rdenph"
  zone      = "ru-central1-b"
}

resource "yandex_container_registry" "default" {
  name      = "ag-registry"
  folder_id = "b1gg3e45d51c39rdenph"
}

resource "yandex_vpc_address" "host-addr" {
  name = "argarden-api-addr"

  external_ipv4_address {
    zone_id = "ru-central1-b"
  }
}

variable "rootPw" {
    type = string
    sensitive = true
}
variable "apiPw" {
    type = string
    sensitive = true
}

resource "yandex_mdb_mongodb_cluster" "argarden-mongo" {
  name        = "argarden"
  environment = "PRODUCTION"
  network_id  = "enpmb31dkvrcidicnopf"

  cluster_config {
    version = "5.0"
  }

  database {
    name = "argarden"
  }

  user {
    name        = "userRw"
    password    = var.rootPw
    permission {
      database_name = "admin"
      roles = [ "mdbShardingManager", "mdbMonitor" ]
    }
    permission {
      database_name = "argarden"
      roles = [ "readWrite" ]
    }
  }

  user {
    name     = "argarden-api"
    password = var.apiPw
    permission {
      database_name = "argarden"
      roles = [ "readWrite" ]
    }
  }

  resources {
    resource_preset_id = "s3-c2-m8"
    disk_size          = 10
    disk_type_id       = "network-hdd"
  }

  host {
    zone_id   = "ru-central1-b"
    subnet_id = "e2lldmaret21q7mgd8s1"
  }

  maintenance_window {
    type = "ANYTIME"
  }
}

resource "yandex_iam_service_account" "docker-puller" {
  name        = "docker-puller"
}

resource "yandex_resourcemanager_folder_iam_binding" "docker-puller-binding" {
  folder_id = "b1gg3e45d51c39rdenph"
  role = "container-registry.images.puller"
  members = [
    "serviceAccount:${yandex_iam_service_account.docker-puller.id}",
  ]
}

resource "yandex_compute_instance" "ag-api-coi" {
  name = "ar-garden-api"
  zone = "ru-central1-b"
  platform_id = "standard-v3"

  boot_disk {
    initialize_params {
      size = 30
    }
  }

  network_interface {
    subnet_id = "e2lldmaret21q7mgd8s1"
    nat_ip_address = yandex_vpc_address.host-addr.external_ipv4_address[0].address
  }

  resources {
    cores = 2
    memory = 2
  }

  metadata = {
    ssh-keys = "rinsz:${file("~/.ssh/id_rsa.pub")}"
    docker-compose = file("../docker-compose.yml")
    serial-port-enable = 1
    user-data = <<-EOT
#cloud-config
datasource:
 Ec2:
  strict_id: false
ssh_pwauth: no
users:
- name: rinsz
  sudo: ALL=(ALL) NOPASSWD:ALL
  shell: /bin/bash
  ssh_authorized_keys:
  - ${file("~/.ssh/id_rsa.pub")}
EOT
  }
}